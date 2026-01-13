<template>
  <v-form ref="form" v-model="valid" @submit.prevent="submit">
    <v-row>
      <v-col
        v-for="field in schema.fields"
        :key="field.key"
        cols="12"
        :md="field.type === 'object-array' ? 12 : 6"
      >
        <!-- Text Input -->
        <v-text-field
          v-if="field.type === 'text'"
          v-model="modelValue[field.key]"
          :label="field.label"
          :rules="getRules(field)"
          variant="outlined"
        ></v-text-field>

        <!-- Textarea -->
        <v-textarea
          v-if="field.type === 'textarea'"
          v-model="modelValue[field.key]"
          :label="field.label"
          :rules="getRules(field)"
          variant="outlined"
        ></v-textarea>

        <!-- Select -->
        <v-select
          v-if="field.type === 'select'"
          v-model="modelValue[field.key]"
          :items="field.options"
          :label="field.label"
          :rules="getRules(field)"
          :multiple="field.multiple"
          variant="outlined"
          item-title="text"
          item-value="value"
        ></v-select>

        <!-- Autocomplete -->
        <v-autocomplete
          v-if="field.type === 'autocomplete'"
          v-model="modelValue[field.key]"
          :items="field.options"
          :label="field.label"
          :rules="getRules(field)"
          :multiple="field.multiple"
          variant="outlined"
          item-title="text"
          item-value="value"
          chips
          closable-chips
        ></v-autocomplete>

        <!-- Object Array (e.g. LocalizedString) -->
        <div v-if="field.type === 'object-array'" class="border pa-4 rounded">
          <div class="d-flex justify-space-between align-center mb-2">
            <h3>{{ field.label }}</h3>
            <v-btn size="small" color="primary" @click="addItem(field.key, field.itemSchema)">
              Add Item
            </v-btn>
          </div>
          
          <div v-if="field.itemSchema">
            <div
              v-for="(item, index) in modelValue[field.key]"
              :key="index"
              class="d-flex align-center gap-2 mb-2"
            >
              <v-row dense class="flex-grow-1">
                <v-col
                  v-for="subField in field.itemSchema.fields"
                  :key="subField.key"
                  cols="12"
                  md="6"
                >
                  <v-text-field
                    v-if="subField.type === 'text'"
                    v-model="item[subField.key]"
                    :label="subField.label"
                    :rules="getRules(subField)"
                    density="compact"
                    variant="outlined"
                    hide-details="auto"
                  ></v-text-field>
                  <v-textarea
                    v-if="subField.type === 'textarea'"
                    v-model="item[subField.key]"
                    :label="subField.label"
                    :rules="getRules(subField)"
                    density="compact"
                    variant="outlined"
                    hide-details="auto"
                    rows="3"
                  ></v-textarea>
                   <v-select
                    v-if="subField.type === 'select'"
                    v-model="item[subField.key]"
                    :items="subField.options"
                    :label="subField.label"
                    :rules="getRules(subField)"
                    density="compact"
                    variant="outlined"
                    hide-details="auto"
                  ></v-select>
                </v-col>
              </v-row>
              <v-btn
                icon="mdi-delete"
                size="small"
                color="error"
                variant="text"
                @click="removeItem(field.key, index)"
              ></v-btn>
            </div>
          </div>
        </div>
      </v-col>
    </v-row>

    <div class="d-flex justify-end mt-4">
      <slot name="actions" :valid="valid" :submit="submit">
        <v-btn color="primary" type="submit" :disabled="!valid">Save</v-btn>
      </slot>
    </div>
  </v-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { FormSchema, FormField } from '@/types/entities';

interface Props {
  modelValue: any;
  schema: FormSchema;
}

const props = defineProps<Props>();
const emit = defineEmits(['update:modelValue', 'submit']);

const valid = ref(false);
const form = ref<any>(null);

const getRules = (field: FormField) => {
  const rules = [];
  if (field.required) {
    rules.push((v: any) => !!v || `${field.label} is required`);
  }
  return rules;
};

const addItem = (key: string, itemSchema?: FormSchema) => {
  if (!props.modelValue[key]) {
    props.modelValue[key] = [];
  }
  
  const newItem: any = {};
  if (itemSchema) {
    itemSchema.fields.forEach(f => {
      newItem[f.key] = f.type === 'array' ? [] : '';
    });
  }
  props.modelValue[key].push(newItem);
};

const removeItem = (key: string, index: number | string) => {
  props.modelValue[key].splice(index as number, 1);
};

const submit = async () => {
    const { valid } = await form.value.validate()
    if (valid) emit('submit', props.modelValue);
};
</script>
