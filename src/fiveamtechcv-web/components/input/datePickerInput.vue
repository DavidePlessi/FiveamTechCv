<template>
  <v-date-picker
      v-model="date"
      :label="label"
  />
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import {ticksToDate, dateToTicks} from "~/utils/dateUtilities";

const props = defineProps<{
  label: string;
  value: string | Date | number;
}>();

const value = ref(props.value);

if (typeof props.value === 'number') {
  value.value = ticksToDate(props.value);
}

const date = computed({
  get: () => value.value,
  set: (newVal) => {
    if (typeof props.value === 'number') {
      value.value = dateToTicks(newVal);
    } else if (typeof props.value === 'string') {
      value.value = new Date(newVal);
    } else {
      value.value = newVal;
    }
  }
});

watch(date, (newVal) => {
  emit('update:modelValue', newVal);
});
</script>