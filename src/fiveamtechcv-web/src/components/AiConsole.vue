<template>
  <div v-if="isOpen" class="ai-console-overlay" ref="overlayRef">
    <div class="ai-console-terminal">
      <div class="terminal-header">
        <span class="terminal-title">System AI Console</span>
        <button @click="close" class="close-btn">x</button>
      </div>
      <div class="terminal-body" ref="chatBody">
        <div v-for="(msg, index) in messages" :key="index" class="message">
          <span class="prompt">{{ msg.isUser ? '> User:' : '> System:' }}</span>
          <!-- Use v-html for system messages to render markdown, v-text for user messages for safety -->
          <span v-if="msg.isUser" class="text">{{ msg.text }}</span>
          <span v-else class="text system-text" v-html="renderMarkdown(msg.text)"></span>
        </div>
        <div v-if="isRecaptchaLoading" class="message">
           <span class="prompt">> System:</span>
           <span class="text blink">Verifying...</span>
        </div>
        <div v-if="isLoading" class="message">
          <span class="prompt">> System:</span>
          <span class="text blink">Processing...</span>
        </div>
      </div>
      <div class="terminal-input">
        <span class="prompt">></span>
        <input
          v-model="userInput"
          @keyup.enter="sendMessage"
          type="text"
          placeholder="Ask a question..."
          ref="inputField"
          :disabled="isLoading"
        />
        <button @click="sendMessage" class="send-btn" :disabled="isLoading || isRecaptchaLoading">SEND</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { aiService } from '@/services/aiService';
import { parseMarkdown } from '@/utils/markdown';
import { v4 as uuidv4 } from 'uuid';

const props = defineProps<{
  isOpen: boolean;
}>();

const emit = defineEmits(['close']);

const messages = ref<{ text: string; isUser: boolean }[]>([
  { text: 'Welcome to System AI. How can I assist you today?', isUser: false }
]);
const HISTORY_KEY = 'fiveamtech_ai_history'; // Key for localStorage
const SESSION_ID_KEY = 'fiveamtech_ai_session_id'; // Key for session id
const userInput = ref('');
const isLoading = ref(false);
const isRecaptchaLoading = ref(false);
const chatBody = ref<HTMLElement | null>(null);
const inputField = ref<HTMLInputElement | null>(null);
const overlayRef = ref<HTMLElement | null>(null);
const sessionId = ref('');

const handleVisualViewportResize = () => {
    if (!overlayRef.value || !window.visualViewport) return;

    // Adjust height to match visual viewport (visible area above keyboard)
    overlayRef.value.style.height = `${window.visualViewport.height}px`;
    // On mobile, sometimes top offset is needed if scrolled
    overlayRef.value.style.top = `${window.visualViewport.offsetTop}px`;
};

const close = () => {
  emit('close');
};

const scrollToBottom = () => {
  nextTick(() => {
    if (chatBody.value) {
      chatBody.value.scrollTop = chatBody.value.scrollHeight;
    }
  });
};

const renderMarkdown = (text: string) => {
  return parseMarkdown(text);
};

// Load history from localStorage on mount
onMounted(() => {
  const savedHistory = localStorage.getItem(HISTORY_KEY);
  if (savedHistory) {
    try {
      messages.value = JSON.parse(savedHistory);
      // Ensure the welcome message is always there if history was somehow empty or corrupted
      if (messages.value.length === 0) {
          messages.value.push({ text: 'Welcome to System AI. How can I assist you today?', isUser: false });
      }
    } catch (e) {
      console.error('Failed to parse history', e);
    }
  }
   scrollToBottom();

   // Load or create Session ID
   let savedSessionId = localStorage.getItem(SESSION_ID_KEY);
   if (!savedSessionId) {
       savedSessionId = uuidv4();
       localStorage.setItem(SESSION_ID_KEY, savedSessionId);
   }
   sessionId.value = savedSessionId;

   // Load reCAPTCHA script
    const siteKey = import.meta.env.VITE_RECAPTCHA_SITE_KEY;
    if (siteKey && !document.getElementById('recaptcha-script')) {
        const script = document.createElement('script');
        script.id = 'recaptcha-script';
        script.src = `https://www.google.com/recaptcha/api.js?render=${siteKey}`;
        script.async = true;
        script.defer = true;
        document.head.appendChild(script);
        document.head.appendChild(script);
    }

    if (window.visualViewport) {
        window.visualViewport.addEventListener('resize', handleVisualViewportResize);
        window.visualViewport.addEventListener('scroll', handleVisualViewportResize);
        // Initial set
        handleVisualViewportResize();
    }
});

onUnmounted(() => {
    if (window.visualViewport) {
        window.visualViewport.removeEventListener('resize', handleVisualViewportResize);
        window.visualViewport.removeEventListener('scroll', handleVisualViewportResize);
    }
});

// Watch messages to save history
watch(messages, (newMessages) => {
  localStorage.setItem(HISTORY_KEY, JSON.stringify(newMessages));
}, { deep: true });

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    scrollToBottom();
    nextTick(() => {
      inputField.value?.focus();
    });
  }
});

const sendMessage = async () => {
  if (!userInput.value.trim() || isLoading.value || isRecaptchaLoading.value) return;

  const question = userInput.value.trim();
  messages.value.push({ text: question, isUser: true });
  userInput.value = '';
  isLoading.value = true;
  scrollToBottom();

  scrollToBottom();

  const siteKey = import.meta.env.VITE_RECAPTCHA_SITE_KEY;

  try {
    isRecaptchaLoading.value = true;

    let token = '';
    // @ts-ignore
    if (window.grecaptcha) {
        // @ts-ignore
        token = await new Promise<string>((resolve) => {
            // @ts-ignore
            window.grecaptcha.ready(() => {
                // @ts-ignore
                window.grecaptcha.execute(siteKey, { action: 'submit' }).then((t: string) => {
                    resolve(t);
                });
            });
        });
    } else {
        console.warn('reCAPTCHA not loaded');
        // Handle failure to load reCAPTCHA - maybe proceed without token and let backend reject?
        // Or show error. For now, proceeding with empty token which will fail backend check.
    }

    isRecaptchaLoading.value = false;

    // Construct history for context (last 10 interactions)
    // We filter out the current question we just added (though it's already in messages)
    // Actually, 'messages' already includes the new question at the end.
    // We want to send the previous context.
    const historyContext = messages.value
        .slice(0, messages.value.length - 1) // Exclude the current question we just pushed
        .slice(-10) // Take last 10
        .map(m => ({role: m.isUser ? 'user' : 'system', text: m.text}));

    const answer = await aiService.ask(question, historyContext, sessionId.value, token);
    messages.value.push({ text: answer, isUser: false });
  } catch (error) {
    console.error('Error:', error);
    messages.value.push({ text: 'Error connecting to AI server.', isUser: false });
  } finally {
    isRecaptchaLoading.value = false;
    isLoading.value = false;
    scrollToBottom();
  }
};
</script>

<style scoped lang="scss">
.ai-console-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.ai-console-terminal {
  width: 80%;
  max-width: 800px;
  height: 60vh;
  border: 1px solid #333;
  border-radius: 8px;
  box-shadow: 0 0 20px rgba(0, 255, 0, 0.2);
  display: flex;
  flex-direction: column;
  font-family: 'Courier New', Courier, monospace;
  color: #0f0;
  overflow: hidden;

  @media (max-width: 768px) {
    width: 100%;
    height: 100%;
    max-width: none;
    border-radius: 0;
    border: none;
  }
}

.terminal-header {
  background-color: #1a1a1a;
  padding: 10px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #333;

  .terminal-title {
    font-weight: bold;
    color: #0f0;
  }

  .close-btn {
    background: none;
    border: none;
    color: #0f0;
    font-size: 1.2rem;
    cursor: pointer;
    &:hover {
      color: #fff;
    }
  }
}

.terminal-body {
  flex-grow: 1;
  padding: 20px;
  overflow-y: auto;
  opacity: 0.9;
  background-color: #0c0c0c;
  .message {
    margin-bottom: 10px;
    line-height: 1.5;

    .prompt {
      color: #0f0;
      margin-right: 10px;
      font-weight: bold;
      vertical-align: top;
    }

    .text {
      color: #ccc;
      display: inline-block;
      &.system-text {
        color: #d0d0d0;

        :deep(strong) {
          font-weight: bold;
          color: #fff;
        }

        :deep(em) {
          font-style: italic;
        }

        :deep(code) {
          background-color: #222;
          padding: 2px 4px;
          border-radius: 3px;
          font-family: 'Courier New', Courier, monospace;
        }

        :deep(pre) {
          background-color: #222;
          padding: 10px;
          border-radius: 5px;
          overflow-x: auto;
          margin: 10px 0;

          code {
            background-color: transparent;
            padding: 0;
          }
        }

        :deep(ul) {
          margin: 5px 0;
          padding-left: 20px;
        }

        :deep(li) {
          margin-bottom: 5px;
        }
      }
    }
  }
}

.terminal-input {
  padding: 10px;
  background-color: #1a1a1a;
  display: flex;
  align-items: center;
  border-top: 1px solid #333;

  .prompt {
    color: #0f0;
    margin-right: 10px;
    font-weight: bold;
  }

  input {
    flex-grow: 1;
    background: transparent;
    border: none;
    color: #fff;
    font-family: 'Courier New', Courier, monospace;
    font-size: 1rem;
    outline: none;

    &::placeholder {
      color: #555;
    }
  }

  .send-btn {
      background-color: transparent;
      color: #0f0;
      border: 1px solid #0f0;
      padding: 5px 10px;
      margin-left: 10px;
      font-family: 'Courier New', Courier, monospace;
      font-weight: bold;
      cursor: pointer;
      text-transform: uppercase;
      font-size: 0.9rem;
      border-radius: 4px;

      &:hover {
          background-color: rgba(0, 255, 0, 0.1);
      }

      &:disabled {
          color: #555;
          border-color: #555;
          cursor: not-allowed;
      }
  }
}

.blink {
  animation: blinker 1s linear infinite;
}

@keyframes blinker {
  50% {
    opacity: 0;
  }
}
</style>
