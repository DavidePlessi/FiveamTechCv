<template>
  <div class="monitor-panel">
    <div class="panel-header">
      <span class="blink-cursor">_</span> {{ title }}
    </div>
    <div class="panel-body">
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
.monitor-panel {
    background: #0a0a0f;
    border: 1px solid #333;
    border-top: 2px solid var(--accent-orange);
    border-radius: 4px;
    overflow: hidden;
    height: 100%;
    min-height: 400px;
    display: flex;
    flex-direction: column;
}

.panel-header {
    background: #111;
    padding: 10px 15px;
    font-size: 0.8rem;
    color: var(--accent-orange);
    border-bottom: 1px solid #333;
    font-weight: bold;
    letter-spacing: 1px;
}

.panel-body {
    padding: 20px;
    flex-grow: 1;
    overflow-y: auto;
    background: repeating-linear-gradient(
        0deg,
        transparent,
        transparent 19px,
        rgba(255, 255, 255, 0.02) 20px
    );
}

/* blink animation is global in cyber.css but safe to inherit or use from global */
</style>
