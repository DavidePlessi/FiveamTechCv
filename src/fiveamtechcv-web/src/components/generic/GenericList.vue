<template>
  <div>
    <v-text-field
      v-model="search"
      label="Search"
      prepend-inner-icon="mdi-magnify"
      variant="outlined"
      density="compact"
      hide-details
      class="mb-4"
    ></v-text-field>

    <!-- Desktop View -->
    <v-data-table
      v-if="!mobile"
      :headers="headers"
      :items="items"
      :loading="loading"
      :search="search"
      class="cyber-table elevation-0"
    >
      <template v-for="(_, name) in $slots" v-slot:[name]="slotData">
        <slot :name="name" v-bind="slotData" />
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
              <slot name="item.actions" :item="item"></slot>
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
  color: var(--accent-orange);
}
.text-dim {
  color: var(--text-dim);
}
.cyber-table {
  background: var(--card-bg) !important;
  backdrop-filter: blur(5px);
  border: 1px solid var(--border);
  border-radius: 8px;
}
</style>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useDisplay } from 'vuetify';

// Props
interface Props {
  items: any[];
  headers: any[];
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
});

const { mobile } = useDisplay();
const search = ref('');

const filteredItems = computed(() => {
  if (!search.value) return props.items;
  const lowerSearch = search.value.toLowerCase();
  return props.items.filter(item => 
    item.name?.toLowerCase().includes(lowerSearch)
  );
});

// Helper to get a title for the card (uses first header that isn't ID or actions, or explicitly 'name' or 'title')
const getTitle = (item: any) => {
  const titleKey = props.headers.find(h => h.key === 'name' || h.key === 'title' || (h.key !== 'id' && h.key !== 'actions'))?.key;
  return titleKey ? item[titleKey] : 'Item';
};
</script>
