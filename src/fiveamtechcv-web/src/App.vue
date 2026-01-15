<template>
  <v-app>
<!--    &lt;!&ndash; Background Effects &ndash;&gt;-->
<!--    <div id="sky-gradient"></div>-->
<!--    <div class="grid-overlay"></div>-->

    <v-main class="pb-16">
      <router-view />
    </v-main>

    <!-- Cyber Dock Navigation -->
    <nav class="cyber-dock">
      <router-link to="/" class="dock-item" :class="{ active: $route.path === '/' }">
        <v-icon>mdi-home</v-icon>
        <span class="dock-tooltip">Dashboard</span>
      </router-link>
      <router-link to="/work-experiences" class="dock-item" :class="{ active: $route.path === '/work-experiences' }">
        <v-icon>mdi-briefcase</v-icon>
        <span class="dock-tooltip">Work Experiences</span>
      </router-link>
      <router-link to="/projects" class="dock-item" :class="{ active: $route.path === '/projects' }">
        <v-icon>mdi-rocket-launch</v-icon>
        <span class="dock-tooltip">Projects</span>
      </router-link>
      <router-link to="/tags" class="dock-item" :class="{ active: $route.path === '/tags' }">
        <v-icon>mdi-tag-multiple</v-icon>
        <span class="dock-tooltip">Tags</span>
      </router-link>
      <router-link to="/graphql" class="dock-item" :class="{ active: $route.path === '/graphql' }">
        <v-icon>mdi-graphql</v-icon>
        <span class="dock-tooltip">GraphQL</span>
      </router-link>

      <a v-if="authStore.isAuthenticated" @click="authStore.logout()" class="dock-item" style="cursor: pointer;">
         <v-icon>mdi-logout</v-icon>
         <span class="dock-tooltip">Logout</span>
      </a>
      <router-link v-else to="/login" class="dock-item" :class="{ active: $route.path === '/login' }">
         <v-icon>mdi-account</v-icon>
         <span class="dock-tooltip">Login</span>
      </router-link>
    </nav>
  </v-app>
</template>

<script lang="ts" setup>
import { useAuthStore } from '@/stores/auth';
const authStore = useAuthStore();
</script>
