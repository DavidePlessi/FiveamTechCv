<template>
  <v-container class="fill-height" fluid>
    <v-row justify="center">
      <v-col cols="12" sm="8" md="4">
        <v-card class="cyber-card elevation-12 pa-4">
          <div class="text-center mb-4">
            <h2 class="text-primary text-uppercase font-weight-bold" style="letter-spacing: 2px;">System Login</h2>
            <div class="subtitle-2 text-dim">Secure Access Terminal</div>
          </div>
          
          <v-card-text>
            <v-form ref="form" v-model="valid" @submit.prevent="handleLogin">
              <v-text-field
                v-model="username"
                label="Identity"
                placeholder="Enter Username"
                variant="outlined"
                bg-color="rgba(0,0,0,0.3)"
                prepend-inner-icon="mdi-account"
                type="text"
                :rules="[v => !!v || 'Username is required']"
                class="mb-2"
              ></v-text-field>

              <v-text-field
                v-model="password"
                id="password"
                label="Passcode"
                placeholder="Enter Password"
                variant="outlined"
                 bg-color="rgba(0,0,0,0.3)"
                prepend-inner-icon="mdi-lock"
                type="password"
                 :rules="[v => !!v || 'Password is required']"
              ></v-text-field>
            </v-form>
          </v-card-text>
          
          <v-card-actions class="justify-center mt-2">
            <v-btn 
                block 
                height="45"
                color="secondary" 
                variant="outlined" 
                class="cyber-btn"
                @click="handleLogin" 
                :disabled="!valid"
            >
                <v-icon start>mdi-login-variant</v-icon>
                Authenticate
            </v-btn>
          </v-card-actions>
        </v-card>
        
        <v-snackbar v-model="error" color="error" variant="tonal" class="text-center">
            <div class="d-flex align-center justify-center">
                <v-icon start icon="mdi-alert-circle"></v-icon>
                <span>Access Denied: Invalid Credentials</span>
            </div>
        </v-snackbar>
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>
.cyber-card {
    background: rgba(20, 20, 25, 0.7) !important;
    backdrop-filter: blur(10px);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-top: 2px solid var(--accent-pink); /* Using CSS variable if available or hardcoded */
    box-shadow: 0 0 20px rgba(0, 0, 0, 0.5);
}

.text-primary {
    color: #d10069 !important; /* Fallback or matches theme */
}

.text-dim {
    color: rgba(255, 255, 255, 0.6);
}

.cyber-btn {
    transition: all 0.3s ease;
    text-transform: uppercase;
    letter-spacing: 1px;
    font-weight: bold;
}

.cyber-btn:hover {
    background: rgba(196, 106, 0, 0.1);
    box-shadow: 0 0 15px rgba(196, 106, 0, 0.4);
}
</style>

<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const username = ref('');
const password = ref('');
const valid = ref(false);
const error = ref(false);
const form = ref<any>(null);

const handleLogin = async () => {
    const { valid: isValid } = await form.value.validate();
    if (!isValid) return;

  if (await authStore.login(username.value, password.value)) {
    router.push('/');
  } else {
    error.value = true;
  }
};
</script>
