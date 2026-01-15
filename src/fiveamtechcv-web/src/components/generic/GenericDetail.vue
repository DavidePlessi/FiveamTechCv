<template>
  <div class="cyber-detail pb-16">
    <v-row>
      <v-col
        v-for="field in schema.fields"
        :key="field.key"
        cols="12"
        :md="(field.type === 'object-array' || (['select', 'autocomplete'].includes(field.type) && field.multiple)) ? 12 : 6"
      >
        <div class="detail-field mb-4">
          <div class="detail-label text-accent-orange mb-1">{{ field.label }}</div>

          <!-- Text / Number / Textarea -->
          <div v-if="['text', 'number', 'textarea'].includes(field.type)" class="detail-value">
            <div :class="{'text-pre-wrap': field.type === 'textarea'}">
                {{ modelValue[field.key] || '-' }}
            </div>
          </div>

          <!-- Select / Autocomplete -->
          <div v-else-if="['select', 'autocomplete'].includes(field.type)">
             <template v-if="field.multiple && Array.isArray(modelValue[field.key])">
                <div
                    class="chip-container"
                    :class="{ 'expanded': expandedFields[field.key] }"
                    ref="containerRefs"
                    :data-key="field.key"
                >
                    <v-chip
                      v-for="(item, i) in modelValue[field.key]"
                      :key="i"
                      class="mr-2 mb-2"
                      variant="outlined"
                    >
                      {{ getOptionLabel(field, item) }}
                    </v-chip>
                    <span v-if="!modelValue[field.key]?.length" class="detail-value-placeholder">-</span>
                </div>
                <div v-if="overflowingFields[field.key]" class="d-flex justify-center mt-1">
                    <v-btn
                        variant="text"
                        size="x-small"
                        color="primary"
                        @click="toggleExpand(field.key)"
                        class="text-none"
                    >
                        {{ expandedFields[field.key] ? 'Show Less' : 'Show More' }}
                        <v-icon :icon="expandedFields[field.key] ? 'mdi-chevron-up' : 'mdi-chevron-down'" end></v-icon>
                    </v-btn>
                </div>
             </template>
             <template v-else>
                <div class="detail-value">
                    {{ getOptionLabel(field, modelValue[field.key]) || '-' }}
                </div>
             </template>
          </div>

          <!-- Date -->
          <div v-else-if="field.type === 'date'" class="detail-value">
             {{ formatDate(modelValue[field.key]) }}
          </div>

          <!-- Object Array -->
          <div v-else-if="field.type === 'object-array'" class="cyber-group-container pa-4 rounded">
             <div v-if="modelValue[field.key] && modelValue[field.key].length > 0">
                <div
                  v-for="(item, index) in modelValue[field.key]"
                  :key="index"
                  class="cyber-group-item mb-3 pa-3"
                >
                   <v-row dense>
                      <v-col
                        v-for="subField in field.itemSchema?.fields"
                        :key="subField.key"
                        cols="12"
                      >
                         <div class="sub-label text-dim text-caption">{{ subField.label }}</div>
                         <div class="sub-value" :class="{'text-pre-wrap': subField.type === 'textarea'}">
                             {{ getDisplayValue(subField, item[subField.key]) }}
                         </div>
                      </v-col>
                   </v-row>
                </div>
             </div>
             <div v-else class="text-dim font-italic">No items</div>
          </div>
        </div>
      </v-col>
    </v-row>

    <div class="d-flex justify-end mt-6">
      <slot name="actions">
        <v-btn color="primary" variant="outlined" @click="$emit('close')" class="cyber-btn">
            Close
        </v-btn>
      </slot>
    </div>
  </div>
</template>

<style scoped>
.cyber-detail {
    font-family: var(--code-font, monospace);
    color: #e0e0e0;
}

.detail-label {
    font-size: 0.85rem;
    text-transform: uppercase;
    letter-spacing: 1px;
    font-weight: 600;
}

.detail-value {
    font-size: 1rem;
    padding: 8px 12px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-radius: 4px;
    min-height: 40px;
    display: flex;
    align-items: center;
}

.detail-value-placeholder {
    padding: 8px 12px;
    display: inline-block;
}

.text-accent-orange {
  color: var(--accent-orange, #ffba08);
}

.text-dim {
  color: rgba(255, 255, 255, 0.6);
}

.text-pre-wrap {
    white-space: pre-wrap;
}

/* Chip Container */
.chip-container {
    max-height: 100px;
    overflow: hidden;
    position: relative;
    transition: max-height 0.3s ease;
    padding: 8px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-radius: 4px;
}

.chip-container.expanded {
    max-height: 1000px; /* Large enough */
    overflow: visible;
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
    background: var(--accent-pink, #f72585);
    opacity: 0.5;
}

.cyber-group-item {
    background: rgba(0, 0, 0, 0.2);
    border: 1px solid rgba(255, 255, 255, 0.05);
    border-radius: 4px;
}

.sub-value {
    font-size: 0.95rem;
    color: #eee;
}

.cyber-btn {
    font-family: var(--code-font, monospace);
    letter-spacing: 1px;
    text-transform: uppercase;
}
</style>

<script setup lang="ts">
import { ref, onMounted, nextTick, watch } from 'vue';
import type { FormSchema, FormField } from '@/types/entities';
import { formatDate } from '@/utils/date';

interface Props {
  modelValue: any;
  schema: FormSchema;
}

const props = defineProps<Props>();
defineEmits(['close']);

const containerRefs = ref<HTMLElement[]>([]);
const overflowingFields = ref<Record<string, boolean>>({});
const expandedFields = ref<Record<string, boolean>>({});

const toggleExpand = (key: string) => {
    expandedFields.value[key] = !expandedFields.value[key];
};

const checkOverflow = async () => {
    await nextTick();
    containerRefs.value.forEach(el => {
        const key = el.dataset.key;
        if (!key) return;

        if (!expandedFields.value[key]) {
             // Use scrollHeight > clientHeight to detect overflow
             // Adding a small buffer (1px) to avoid false positives due to sub-pixel rendering
             if (el.scrollHeight > el.clientHeight + 1) {
                overflowingFields.value[key] = true;
             } else {
                overflowingFields.value[key] = false;
             }
        }
    });
};

watch(() => props.modelValue, checkOverflow, { deep: true, immediate: true });
onMounted(() => {
    checkOverflow();
    window.addEventListener('resize', checkOverflow);
});

const getOptionLabel = (field: FormField, value: any) => {
    if (value === null || value === undefined) return '';
    if (!field.options) return value;

    const option = field.options.find(opt => opt.value === value);
    return option ? option.text : value;
};


const getDisplayValue = (field: FormField, value: any) => {
    if (['select', 'autocomplete'].includes(field.type)) {
        return getOptionLabel(field, value);
    }
    if (field.type === 'date') {
        return formatDate(value);
    }
    return value || '-';
};
</script>
