<template>
  <v-chip
    size="small"
    class="mr-1 mb-1 cyber-chip"
    color="secondary"
    variant="outlined"
    label
    v-bind="$attrs"
  >
    <slot>
        <v-icon v-if="icon" start size="x-small">{{ icon }}</v-icon>
        {{ text }}
    </slot>
  </v-chip>
</template>

<script setup lang="ts">
defineProps<{
  text?: string;
  icon?: string;
}>();
</script>

<style scoped>
/* Styles mapped to global cyber.css */
</style>
