<template>
  <v-text-field
      v-model="text"
      :label="label"
      outlined
  />
</template>

<script setup lang="ts">
import { ref } from 'vue';

const props = defineProps<{
  label: string;
  value: string;
}>();


const text = ref(props.value);

watch(text, (newVal) => {
  emit('update:modelValue', newVal);
});
</script>