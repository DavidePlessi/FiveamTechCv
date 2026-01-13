<template>


      <v-container fluid class="pb-16">
        <router-view></router-view>
      </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth';

const drawer = ref(true);
const authStore = useAuthStore();
</script>
