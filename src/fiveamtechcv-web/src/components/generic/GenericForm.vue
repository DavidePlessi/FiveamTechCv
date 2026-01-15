<template>
  <v-form ref="form" v-model="valid" @submit.prevent="submit" class="cyber-form  pb-16">
    <v-row>
      <v-col
        v-for="field in schema.fields"
        :key="field.key"
        cols="12"
        :md="field.type === 'object-array' || (['select', 'autocomplete'].includes(field.type) && field.multiple) ? 12 : 6"
      >
        <!-- Text Input -->
        <v-text-field
          v-if="field.type === 'text'"
          v-model="modelValue[field.key]"
          :label="field.label"
          :rules="getRules(field)"
          variant="outlined"
          class="cyber-input"
          density="comfortable"
        ></v-text-field>

        <!-- Number Input -->
        <v-text-field
          v-if="field.type === 'number'"
          v-model.number="modelValue[field.key]"
          :label="field.label"
          :rules="getRules(field)"
          type="number"
          variant="outlined"
          class="cyber-input"
          density="comfortable"
        ></v-text-field>

        <!-- Textarea -->
        <v-textarea
          v-if="field.type === 'textarea'"
          v-model="modelValue[field.key]"
          :label="field.label"
          :rules="getRules(field)"
          variant="outlined"
          class="cyber-input"
          density="comfortable"
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
          class="cyber-input"
          density="comfortable"
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
          class="cyber-input"
          density="comfortable"
        ></v-autocomplete>

        <!-- Date Picker -->
        <cyber-datepicker
            v-if="field.type === 'date'"
            v-model="modelValue[field.key]"
            :label="field.label"
            :rules="getRules(field)"
        ></cyber-datepicker>

        <!-- Object Array (e.g. LocalizedString) -->
        <div v-if="field.type === 'object-array'" class="cyber-group-container pa-2 rounded">
          <div class="d-flex justify-space-between align-center mb-4 pl-4 pr-4">
            <h3 class="cyber-group-title">{{ field.label }}</h3>
            <v-btn size="small" color="primary" variant="outlined" class="cyber-btn" @click="addItem(field.key, field.itemSchema)">
              <v-icon start>mdi-plus</v-icon> Add Item
            </v-btn>
          </div>

          <div v-if="field.itemSchema">
            <div
              v-for="(item, index) in modelValue[field.key]"
              :key="index"
              class="cyber-group-item d-flex  align-start gap-2 mb-3 "
            >
              <v-col dense class="flex-grow-1 pa-0">
                <v-col
                  v-for="subField in field.itemSchema.fields"
                  :key="subField.key"
                  cols="12"
                >
                  <v-text-field
                    v-if="subField.type === 'text'"
                    v-model="item[subField.key]"
                    :label="subField.label"
                    :rules="getRules(subField)"
                    density="compact"
                    variant="outlined"
                    hide-details="auto"
                    class="cyber-input"
                  ></v-text-field>
                  <v-text-field
                    v-if="subField.type === 'number'"
                    v-model.number="item[subField.key]"
                    :label="subField.label"
                    :rules="getRules(subField)"
                    type="number"
                    density="compact"
                    variant="outlined"
                    hide-details="auto"
                    class="cyber-input"
                  ></v-text-field>
                  <v-textarea
                    v-if="subField.type === 'textarea'"
                    v-model="item[subField.key]"
                    :label="subField.label"
                    :rules="getRules(subField)"
                    density="compact"
                    variant="outlined"
                    hide-details="auto"
                    rows="5"
                    class="cyber-input"
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
                    class="cyber-input"
                  ></v-select>
                </v-col>
                <v-col class="d-flex justify-end">
                  <v-btn
                    icon="mdi-delete"
                    size="small"
                    color="error"
                    variant="text"
                    class="mt-1"
                    @click="removeItem(field.key, index)"
                  ></v-btn>
                </v-col>
              </v-col>
            </div>
          </div>
        </div>
      </v-col>
    </v-row>

    <div class="d-flex justify-end mt-6">
      <slot name="actions" :valid="valid" :submit="submit">
        <v-btn color="primary" type="submit" :disabled="!valid" class="cyber-btn-glow" size="large">
            <v-icon start>mdi-content-save</v-icon> Save
        </v-btn>
      </slot>
    </div>
  </v-form>
</template>

<style scoped>
.cyber-form {
    font-family: var(--code-font, monospace);
}

/* Input Styles */
:deep(.v-field__outline__start),
:deep(.v-field__outline__end),
:deep(.v-field__outline__notch::before),
:deep(.v-field__outline__notch::after) {
    border-color: rgba(255, 255, 255, 0.2) !important;
}

:deep(.v-field--focused .v-field__outline__start),
:deep(.v-field--focused .v-field__outline__end),
:deep(.v-field--focused .v-field__outline__notch::before),
:deep(.v-field--focused .v-field__outline__notch::after) {
    border-color: var(--accent-orange) !important;
    box-shadow: 0 0 5px rgba(196, 106, 0, 0.3);
}

/* Fix for the shadow cutting off the label area */
:deep(.v-field--focused .v-field__outline__notch::before),
:deep(.v-field--focused .v-field__outline__notch::after) {
    box-shadow: none !important; /* Remove shadow from notch parts to avoid cut-off look */
}

/* Apply shadow only to the main outline parts */
:deep(.v-field--focused .v-field__outline__start) {
    box-shadow: -2px 0 5px rgba(196, 106, 0, 0.3) !important;
}
:deep(.v-field--focused .v-field__outline__end) {
    box-shadow: 2px 0 5px rgba(196, 106, 0, 0.3) !important;
}


:deep(.v-label) {
    color: var(--text-dim) !important;
    font-family: var(--code-font, monospace);
    letter-spacing: 0.5px;
}

:deep(.v-field__input) {
    color: #fff !important;
    font-family: var(--code-font, monospace);
}

/* Group Container (Object Array) */
.cyber-group-container {
    background: rgba(255, 255, 255, 0.02);
    border: 1px solid rgba(255, 255, 255, 0.1);
    position: relative;
    overflow: hidden;
}

.cyber-group-container::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 4px;
    height: 100%;
    background: var(--accent-pink);
    opacity: 0.5;
}

.cyber-group-title {
    color: var(--accent-orange);
    font-family: var(--code-font, monospace);
    text-transform: uppercase;
    letter-spacing: 1px;
    font-size: 0.9rem;
}

.cyber-group-item {
    background: rgba(0, 0, 0, 0.2);
    border: 1px solid rgba(255, 255, 255, 0.05);
    border-radius: 4px;
}

/* Buttons */
.cyber-btn {
    font-family: var(--code-font, monospace);
    letter-spacing: 1px;
    text-transform: uppercase;
}

.cyber-btn-glow {
    font-family: var(--code-font, monospace);
    letter-spacing: 2px;
    text-transform: uppercase;
    font-weight: bold;
    box-shadow: 0 0 15px rgba(var(--v-theme-primary), 0.4);
    transition: all 0.3s ease;
}

.cyber-btn-glow:hover {
    box-shadow: 0 0 25px rgba(var(--v-theme-primary), 0.6);
    transform: translateY(-2px);
}
</style>

<script setup lang="ts">
import { ref } from 'vue';
import type { FormSchema, FormField } from '@/types/entities';
import CyberDatepicker from '@/components/shared/CyberDatepicker.vue';

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
