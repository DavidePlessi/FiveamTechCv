<template>
  <v-form @submit.prevent="submitForm">
    <div v-for="key in keys" :key="key">
      <generic-input :is-date-field="false" :label="key" :value="undefined" />
    </div>
    <v-btn type="submit">Submit</v-btn>
  </v-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import {BaseEntity, getPropertyNames} from "~/entities/entities";
import GenericInput from "~/components/input/genericInput.vue";

const props = defineProps<{
  entityClass: { new(): BaseEntity; };
  value: BaseEntity;
  submitForm: (data: BaseEntity) => Promise<void>;
}>();

const entity = ref(new props.entityClass());
const keys = getPropertyNames(entity.value);

console.log(keys);

const submitForm = async () => {
  await props.submitForm(entity.value);
};
</script>