<template>
  <div class="cyber-filter mb-4">
    <div class="d-flex align-center gap-2 flex-wrap">
      <!-- Property Select -->
      <v-select
        v-model="selectedFieldKey"
        :items="props.schema.fields"
        item-title="label"
        item-value="key"
        label="Filter By"
        variant="outlined"
        density="compact"
        hide-details
        class="cyber-input filter-select"
        style="min-width: 150px; flex: 0 1 auto;"
        @update:model-value="resetFilterValue"
      ></v-select>

      <!-- Dynamic Input -->
      <template v-if="selectedField">
        <v-text-field
          v-if="selectedField.type === 'text'"
          v-model="filterValue"
          label="Value"
          variant="outlined"
          density="compact"
          hide-details
          class="cyber-input"
          style="min-width: 200px; flex: 1;"
          @keyup.enter="addFilter"
        ></v-text-field>

        <!-- Select (Small Lists) -->
        <v-select
          v-else-if="selectedField.type === 'select' && (!selectedField.options || selectedField.options.length <= 10)"
          v-model="filterValue"
          :items="selectedField.options"
          item-title="text"
          item-value="value"
          label="Select Value"
          variant="outlined"
          density="compact"
          hide-details
          class="cyber-input"
          style="min-width: 200px; flex: 1;"
          @update:model-value="addFilter"
        ></v-select>

        <!-- Autocomplete (Large Lists) -->
        <v-autocomplete
          v-else-if="selectedField.type === 'select'"
          v-model="filterValue"
          :items="selectedField.options"
          item-title="text"
          item-value="value"
          label="Search Value"
          variant="outlined"
          density="compact"
          hide-details
          class="cyber-input"
          style="min-width: 200px; flex: 1;"
          @update:model-value="addFilter"
        ></v-autocomplete>

         <v-text-field
          v-else-if="selectedField.type === 'number'"
          v-model.number="filterValue"
          label="Value"
          type="number"
          variant="outlined"
          density="compact"
          hide-details
          class="cyber-input"
          style="min-width: 200px; flex: 1;"
          @keyup.enter="addFilter"
        ></v-text-field>

         <!-- Fallback for other types -->
        <v-text-field
          v-else
          v-model="filterValue"
          label="Value"
          variant="outlined"
          density="compact"
          hide-details
          class="cyber-input"
          style="min-width: 200px; flex: 1;"
          @keyup.enter="addFilter"
        ></v-text-field>
      </template>

      <v-btn
        color="primary"
        icon="mdi-plus"
        variant="text"
        :disabled="!isValidFilter"
        @click="addFilter"
      ></v-btn>
    </div>

    <!-- Active Filters Chips -->
    <div class="mt-2 d-flex flex-wrap gap-2" v-if="activeFilters.length > 0">
      <v-chip
        v-for="(filter, index) in activeFilters"
        :key="index"
        closable
        color="secondary"
        label
        variant="outlined"
        class="cyber-chip"
        @click:close="removeFilter(index)"
      >
        <strong class="mr-1">{{ filter.label }}:</strong>
        <span>{{ getDisplayValue(filter) }}</span>
      </v-chip>
      
      <v-btn 
        v-if="activeFilters.length > 1" 
        variant="text" 
        size="small" 
        color="error" 
        class="ms-2 align-self-center"
        @click="clearAll"
      >
        Clear All
      </v-btn>
    </div>
  </div>
</template>

<style scoped>
.gap-2 {
  gap: 8px;
}

.cyber-filter {
  background: rgba(255, 189, 46, 0.1);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 189, 46, 0.5);
  box-shadow: 0 0 25px rgba(255, 189, 46, 0.15), inset 0 0 10px rgba(255, 189, 46, 0.05);
  border-radius: 12px;
  padding: 20px;
  transition: all 0.3s ease;
  position: relative;
  z-index: 5;
}

.cyber-filter:hover {
  border-color: rgba(255, 189, 46, 0.8);
  box-shadow: 0 0 30px rgba(255, 189, 46, 0.25), inset 0 0 20px rgba(255, 189, 46, 0.1);
}

/* Reusing generic cyber styles - ideally these should be global or imported */
.cyber-input :deep(.v-field__outline__start),
.cyber-input :deep(.v-field__outline__end),
.cyber-input :deep(.v-field__outline__notch::before),
.cyber-input :deep(.v-field__outline__notch::after) {
    border-color: rgba(255, 255, 255, 0.2) !important;
}

.cyber-chip {
    border-color: #ff4081 !important;
    color: #ff4081 !important;
    background: rgba(255, 64, 129, 0.1) !important;
}
</style>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import type { FormSchema, FormField } from '@/types/entities';

interface FilterItem {
  key: string;
  label: string;
  value: any;
  type: string;
  displayValue?: string; // Optional custom display text
}

interface Props {
  schema: FormSchema;
}

const props = defineProps<Props>();
const emit = defineEmits(['update:filters']);

const selectedFieldKey = ref<string | null>(null);
const filterValue = ref<any>(null);
const activeFilters = ref<FilterItem[]>([]);

const selectedField = computed(() => {
  return props.schema.fields.find(f => f.key === selectedFieldKey.value);
});

const isValidFilter = computed(() => {
  return selectedField.value && 
         (filterValue.value !== null && filterValue.value !== '' && filterValue.value !== undefined);
});

const resetFilterValue = () => {
    filterValue.value = null;
};

const getDisplayValue = (filter: FilterItem) => {
    if (filter.type === 'select') {
        // Find the text representation if it's a select
        const field = props.schema.fields.find(f => f.key === filter.key);
        if (field && field.options) {
             const option = field.options.find(o => o.value === filter.value);
             return option ? option.text : filter.value;
        }
    }
    return filter.value;
};

const addFilter = () => {
  if (!isValidFilter.value || !selectedField.value) return;

  const newFilter: FilterItem = {
    key: selectedField.value.key,
    label: selectedField.value.label,
    value: filterValue.value,
    type: selectedField.value.type
  };

  // Check if filter already exists? Or allow multiple of same type?
  // Let's allow multiple (OR logic usually, or AND? The prompt implies "the filter inserted", typically list filters are AND)
  // But duplicate exact filter should be avoided
  const exists = activeFilters.value.some(f => f.key === newFilter.key && f.value === newFilter.value);
  if (!exists) {
      activeFilters.value.push(newFilter);
      emit('update:filters', activeFilters.value);
  }
  
  // Optional: clear value but keep field selected for rapid entry? 
  // Or reset everything? Let's reset value.
  filterValue.value = null;
};

const removeFilter = (index: number) => {
  activeFilters.value.splice(index, 1);
  emit('update:filters', activeFilters.value);
};

const clearAll = () => {
    activeFilters.value = [];
    emit('update:filters', activeFilters.value);
};

onMounted(() => {
  if (props.schema && props.schema.fields && props.schema.fields.length > 0) {
    const firstField = props.schema.fields[0];
    if (firstField) {
        selectedFieldKey.value = firstField.key;
    }
  }
});
</script>
