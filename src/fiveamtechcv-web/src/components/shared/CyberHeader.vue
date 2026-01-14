<template>
  <div class="d-flex flex-column flex-md-row justify-space-between align-start align-md-center mb-4">
    <div class="d-flex align-center mb-4 mb-md-0">
      <img src="@/assets/logo-nobg.png" alt="Logo" class="view-logo mr-4" />
      <div>
        <h1 class="cyber-title">{{ title }}</h1>
        <div v-if="subtitle || $slots.subtitle" class="text-subtitle-2 text-md-subtitle-1 text-medium-emphasis mt-1">
          <slot name="subtitle">{{ subtitle }}</slot>
        </div>
      </div>
    </div>
    <div class="align-self-end align-self-md-center">
        <slot></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  title: string;
  subtitle?: string;
}>();
</script>

<style scoped>
/* Styles are now in cyber.css, but we keep this empty or for component specific overrides if needed */
</style>
