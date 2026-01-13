<template>
  <v-switch
      v-model="value"
      :label="label"
  />
</template>

<script setup lang="ts">
import { ref } from 'vue';

const props = defineProps<{
  label: string;
  value: boolean;
}>();

const value = ref(props.value);
watch(value, (newVal) => {
  emit('update:modelValue', newVal);
});
</script>