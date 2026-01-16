<template>
  <div v-if="isOpen" class="ai-console-overlay">
    <div class="ai-console-terminal">
      <div class="terminal-header">
        <span class="terminal-title">FiveamTech AI Console</span>
        <button @click="close" class="close-btn">x</button>
      </div>
      <div class="terminal-body" ref="chatBody">
        <div v-for="(msg, index) in messages" :key="index" class="message">
          <span class="prompt">{{ msg.isUser ? '> User:' : '> System:' }}</span>
          <span class="text" :class="{ 'system-text': !msg.isUser }">{{ msg.text }}</span>
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
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { aiService } from '@/services/aiService';

const props = defineProps<{
  isOpen: boolean;
}>();

const emit = defineEmits(['close']);

const messages = ref<{ text: string; isUser: boolean }[]>([
  { text: 'Welcome to FiveamTech AI. How can I assist you today?', isUser: false }
]);
const userInput = ref('');
const isLoading = ref(false);
const chatBody = ref<HTMLElement | null>(null);
const inputField = ref<HTMLInputElement | null>(null);

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

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    scrollToBottom();
    nextTick(() => {
      inputField.value?.focus();
    });
  }
});

const sendMessage = async () => {
  if (!userInput.value.trim() || isLoading.value) return;

  const question = userInput.value.trim();
  messages.value.push({ text: question, isUser: true });
  userInput.value = '';
  isLoading.value = true;
  scrollToBottom();

  try {
    const answer = await aiService.ask(question);
    messages.value.push({ text: answer, isUser: false });
  } catch (error) {
    console.error('Error:', error);
    messages.value.push({ text: 'Error connecting to AI server.', isUser: false });
  } finally {
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
  width: 100vw;
  height: 100vh;
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
    width: 100vw;
    height: 100vh;
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
    }

    .text {
      color: #ccc;
      &.system-text {
        color: #0f0;
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
