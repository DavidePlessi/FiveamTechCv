<template>
  <div class="stat-card">
    <div class="stat-icon">
      <v-icon size="large" :color="iconColor">{{ icon }}</v-icon>
    </div>
    <div class="stat-content">
      <div class="stat-value">{{ value }}</div>
      <div class="stat-label">{{ label }}</div>
    </div>
    <div class="corner-accent top-right"></div>
    <div class="corner-accent bottom-left"></div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  icon: string;
  iconColor?: string;
  value: string | number;
  label: string;
}>();
</script>

<style scoped>
.stat-card {
    background: rgba(20, 20, 25, 0.6);
    border: 1px solid rgba(255, 255, 255, 0.1);
    padding: 20px;
    display: flex;
    align-items: center;
    gap: 20px;
    position: relative;
    overflow: hidden;
    backdrop-filter: blur(5px);
}

.stat-icon {
    background: rgba(255, 255, 255, 0.05);
    padding: 15px;
    border-radius: 50%;
    border: 1px solid rgba(255, 255, 255, 0.1);
}

.stat-value {
    font-size: 2rem;
    font-weight: bold;
    color: #fff;
    line-height: 1;
}

.stat-label {
    color: var(--text-dim);
    font-size: 0.9rem;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.corner-accent {
    position: absolute;
    width: 10px;
    height: 10px;
    border: 2px solid var(--accent-orange);
    opacity: 0.5;
}

.top-right { top: 0; right: 0; border-bottom: none; border-left: none; }
.bottom-left { bottom: 0; left: 0; border-top: none; border-right: none; }
</style>
