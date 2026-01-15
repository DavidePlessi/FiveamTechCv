<template>
  <div>
    <generic-filter
      v-if="filterSchema"
      :schema="filterSchema"
      @update:filters="updateFilters"
    />

    <!-- Desktop View -->
    <v-data-table
      v-if="!mobile"
      :headers="headers"
      :items="filteredItems"
      :loading="loading"
      class="cyber-table elevation-0"
    >
      <template v-for="(_, name) in $slots" v-slot:[name]="slotData">
        <slot :name="name" v-bind="slotData" />
      </template>

      <!-- Default Actions Slot if not overridden -->
      <template v-if="!$slots['item.actions']" v-slot:item.actions="{ item }">
         <div class="d-flex justify-end">
            <v-btn
              icon="mdi-eye"
              variant="text"
              size="small"
              color="info"
              class="mr-2"
              @click="$emit('detail', item)"
            ></v-btn>
            <v-btn
              icon="mdi-pencil"
              variant="text"
              size="small"
              color="primary"
              class="mr-2"
              @click="$emit('edit', item)"
            ></v-btn>
            <v-btn
              icon="mdi-delete"
              variant="text"
              size="small"
              color="error"
              @click="$emit('delete', item)"
            ></v-btn>
         </div>
      </template>
    </v-data-table>

    <!-- Mobile View -->
    <div v-else>
      <v-skeleton-loader v-if="loading" type="card@3" class="bg-transparent"></v-skeleton-loader>
      <v-row v-else>
        <v-col
          v-for="(item, index) in filteredItems"
          :key="item.id || index"
          cols="12"
          sm="6"
        >
          <div class="holo-card mb-4">
            <div class="card-header">
              <h4>{{ getTitle(item) }}</h4>
            </div>
            <div class="card-body">
              <div v-for="header in headers" :key="header.key">
                <div v-if="header.key !== 'actions' && header.key !== 'title' && header.key !== 'name'" class="mb-2">
                  <strong class="text-accent-orange">{{ header.title }}:</strong>
                  <div v-if="$slots['item.' + header.key]">
                    <slot :name="'item.' + header.key" :item="item"></slot>
                  </div>
                  <span v-else class="text-dim"> {{ item[header.key] }}</span>
                </div>
              </div>
            </div>
            <div class="card-actions mt-3 d-flex justify-end">
              <slot name="item.actions" :item="item">
                 <v-btn
                    icon="mdi-eye"
                    variant="text"
                    size="small"
                    color="info"
                    class="mr-2"
                    @click="$emit('detail', item)"
                  ></v-btn>
                  <v-btn
                    icon="mdi-pencil"
                    variant="text"
                    size="small"
                    color="primary"
                    class="mr-2"
                    @click="$emit('edit', item)"
                  ></v-btn>
                  <v-btn
                    icon="mdi-delete"
                    variant="text"
                    size="small"
                    color="error"
                    @click="$emit('delete', item)"
                  ></v-btn>
              </slot>
            </div>

            <!-- Deco Corners -->
            <div class="corner-accent top-right"></div>
            <div class="corner-accent bottom-left"></div>
          </div>
        </v-col>
      </v-row>
    </div>
  </div>
</template>

<style scoped>
.text-accent-orange {
  color: var(--accent-orange, #ffba08);
}
.text-dim {
  color: rgba(255, 255, 255, 0.7);
}

/* Table Styling */
.cyber-table {
  background: rgba(10, 10, 15, 0.6) !important;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 189, 46, 0.1);
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 0 20px rgba(0, 0, 0, 0.5);
}

.cyber-table :deep(thead tr th) {
  background: rgba(255, 189, 46, 0.05) !important;
  color: var(--accent-orange) !important;
  text-transform: uppercase;
  letter-spacing: 1px;
  border-bottom: 1px solid rgba(255, 189, 46, 0.2) !important;
  font-weight: 600;
}

.cyber-table :deep(tbody tr:hover) {
  background: rgba(255, 189, 46, 0.05) !important;
}

.cyber-table :deep(tbody tr td) {
  border-bottom: 1px solid rgba(255, 255, 255, 0.05) !important;
  color: #e0e0e0;
}

/* Mobile Card Styling */
.holo-card {
  background: rgba(10, 10, 15, 0.8);
  border: 1px solid rgba(255, 189, 46, 0.2);
  border-radius: 8px;
  padding: 16px;
  position: relative;
  overflow: hidden;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.5);
  transition: transform 0.2s, box-shadow 0.2s;
}

.holo-card:active {
  transform: scale(0.98);
}

.holo-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
  background: linear-gradient(to bottom, var(--accent-orange), transparent);
}

.card-header h4 {
  color: var(--accent-orange);
  margin-bottom: 8px;
  font-family: inherit;
  text-transform: uppercase;
  letter-spacing: 1px;
  border-bottom: 1px solid rgba(255, 189, 46, 0.1);
  padding-bottom: 4px;
}

.card-body {
  font-size: 0.9rem;
}

.corner-accent {
  position: absolute;
  width: 10px;
  height: 10px;
  border: 2px solid var(--accent-orange);
  opacity: 0.5;
  pointer-events: none;
}

.corner-accent.top-right {
  top: 6px;
  right: 6px;
  border-bottom: none;
  border-left: none;
}

.corner-accent.bottom-left {
  bottom: 6px;
  left: 6px;
  border-top: none;
  border-right: none;
}
</style>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useDisplay } from 'vuetify';
import GenericFilter from './GenericFilter.vue';
import type { FormSchema } from '@/types/entities'; // Ensure this matches user's types

// Props
interface Props {
  items: any[];
  headers: any[];
  loading?: boolean;
  filterSchema?: FormSchema; // Optional schema for advanced filtering
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
});

const emit = defineEmits(['detail', 'edit', 'delete']);

const { mobile } = useDisplay();

const activeFilters = ref<any[]>([]);

const updateFilters = (filters: any[]) => {
    // Create a new array to ensure reactivity trigger if needed, though activeFilters.value assignment should be enough.
    activeFilters.value = [...filters];
};

const filteredItems = computed(() => {
  let items = [...props.items];

  // 1. Apply Advanced Filters
  // Logic: AND between different properties, OR between values of the same property
  if (activeFilters.value.length > 0) {
      // Group filters by key
      const filtersByKey: Record<string, any[]> = {};
      activeFilters.value.forEach(filter => {
          if (!filtersByKey[filter.key]) {
              filtersByKey[filter.key] = [];
          }
          filtersByKey[filter.key]!.push(filter);
      });

      items = items.filter(item => {
          // Check each property group
          return Object.keys(filtersByKey).every(key => {
              const groupFilters = filtersByKey[key] || [];
              const itemValue = item[key]; // This might be an array for some properties? Assuming scalar for now based on current usage.

              // If item doesn't have the property, it fails the filter (unless we want to support "not set")
              if (itemValue === undefined || itemValue === null) return false;

              // OR logic within the group: at least one filter in this group must match
              const match = groupFilters.some(filter => {
                  // String comparison (insensitive partial match)
                  if (typeof itemValue === 'string' && typeof filter.value === 'string') {
                      return itemValue.toLowerCase().includes(filter.value.toLowerCase());
                  }

                  // Number comparison (exact)
                  if (typeof itemValue === 'number' && (typeof filter.value === 'number' || !isNaN(Number(filter.value)))) {
                      return itemValue === Number(filter.value);
                  }

                   // Array handling (e.g. if itemValue is an array of objects (Related Entities) or primitives)
                   if (Array.isArray(itemValue)) {
                       // If filter value is present in the array (primitive check)
                       if (itemValue.includes(filter.value)) return true;

                       // If array contains objects, check relevant properties (id, name)
                       return itemValue.some(child => {
                           if (child && typeof child === 'object') {
                               // Check ID match (common for Select filters)
                               if (child.id === filter.value) return true;
                               // Check Name match (common for Text filters)
                               if (child.name && typeof filter.value === 'string' &&
                                   child.name.toLowerCase().includes(filter.value.toLowerCase())) return true;
                           }
                           return false;
                       });
                   }

                  // Boolean / Exact match for others
                  return itemValue === filter.value;
              });

              return match;
          });
      });
  }



  // 3. Sort by order if available
  // Doing sort last usually better for performance on smaller subset
  items.sort((a, b) => {
    const orderA = a.order ?? Number.MAX_SAFE_INTEGER;
    const orderB = b.order ?? Number.MAX_SAFE_INTEGER;
    return orderA - orderB;
  });

  return items;
});

// Helper to get a title for the card (uses first header that isn't ID or actions, or explicitly 'name' or 'title')
const getTitle = (item: any) => {
  const titleKey = props.headers.find(h => h.key === 'name' || h.key === 'title' || (h.key !== 'id' && h.key !== 'actions' && h.key !== 'order'))?.key;
  return titleKey ? item[titleKey] : 'Item';
};
</script>
