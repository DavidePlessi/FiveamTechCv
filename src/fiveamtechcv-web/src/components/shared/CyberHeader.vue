<template>
  <div class="d-flex justify-space-between align-center mb-4">
    <div class="d-flex align-center">
      <img src="@/assets/logo-nobg.png" alt="Logo" class="view-logo mr-4" />
      <h1 class="cyber-title">{{ title }}</h1>
    </div>
    <div>
        <slot></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  title: string;
}>();
</script>

<style scoped>
/* Styles are now in cyber.css, but we keep this empty or for component specific overrides if needed */
</style>
