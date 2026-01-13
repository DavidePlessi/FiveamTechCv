<template>
  <component
      :is="componentType"
      v-model="value"
      :label="label"
  />
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';

const props = defineProps<{
  label: string;
  value: string | Date | boolean | number;
  isDateField: boolean;
}>();

const value = ref(props.value);

const componentType = computed(() => {
  if (props.isDateField) {
    return 'date-picker-input';
  } else
  
  
  if (typeof props.value === 'boolean') {
    return 'switch-input';
  } else if (props.value instanceof Date) {
    return 'date-picker-input';
  } else {
    return 'text-input';
  }
});

watch(value, (newVal) => {
  emit('update:modelValue', newVal);
});
</script>