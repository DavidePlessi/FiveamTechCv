<template>
  <div>
    <cyber-header title="Companies" subtitle="Manage companies linked to work experiences.">
        <v-btn v-if="authStore.isAuthenticated" color="primary" prepend-icon="mdi-plus" @click="openDialog()">Add Company</v-btn>
    </cyber-header>

    <generic-list
      :items="filteredItems"
      :headers="headers"
      :loading="loading"
      :filter-schema="filterSchema"
      @detail="openDetail"
      @edit="openDialog"
      @delete="deleteItem"
    >
      <template #item.website="{ item }">
        <a v-if="item.website" :href="item.website" target="_blank" class="text-decoration-none text-primary">
            {{ item.website }} <v-icon size="small" icon="mdi-open-in-new"></v-icon>
        </a>
      </template>
      <template #item.description="{ item }">
        <div v-for="desc in item.description" :key="desc.language">
            <strong>{{ desc.language }}:</strong> {{ desc.value ?? '' }}
        </div>
      </template>

      <template #item.actions="{ item }">
        <v-btn icon="mdi-eye" size="small" variant="text" color="info" @click="openDetail(item)"></v-btn>
        <v-btn v-if="authStore.isAuthenticated" icon="mdi-pencil" size="small" variant="text" color="primary" @click="openDialog(item)"></v-btn>
        <v-btn v-if="authStore.isAuthenticated" icon="mdi-delete" size="small" variant="text" color="error" @click="deleteItem(item)"></v-btn>
      </template>
    </generic-list>

    <!-- Edit Dialog -->
    <v-dialog v-model="dialog" max-width="800px" :fullscreen="mobile" :transition="mobile ? 'dialog-bottom-transition' : 'dialog-transition'">
      <v-card>
        <v-toolbar v-if="mobile" color="primary" density="compact">
            <v-btn icon="mdi-close" @click="closeDialog"></v-btn>
            <v-toolbar-title>{{ editedItem.id ? 'Edit Company' : 'New Company' }}</v-toolbar-title>
        </v-toolbar>
        <v-card-title v-else>{{ editedItem.id ? 'Edit Company' : 'New Company' }}</v-card-title>
        <v-card-text>
          <generic-form
            v-if="dialog"
            :model-value="editedItem"
            :schema="schema"
            @submit="save"
          >
             <template #actions="{ valid, submit }">
                <v-btn color="blue-darken-1" variant="text" @click="closeDialog">Cancel</v-btn>
                <v-btn color="blue-darken-1" variant="text" @click="submit" :disabled="!valid">Save</v-btn>
             </template>
          </generic-form>
        </v-card-text>
      </v-card>
    </v-dialog>

    <!-- Detail Dialog -->
    <v-dialog v-model="detailDialog" max-width="800px" :fullscreen="mobile" :transition="mobile ? 'dialog-bottom-transition' : 'dialog-transition'">
      <v-card>
        <v-toolbar v-if="mobile" color="primary" density="compact">
            <v-btn icon="mdi-close" @click="closeDetail"></v-btn>
            <v-toolbar-title>Company Details</v-toolbar-title>
        </v-toolbar>
        <v-card-title v-else>Company Details</v-card-title>
        <v-card-text>
           <generic-detail
             v-if="detailDialog"
             :model-value="detailItem"
             :schema="schema"
             @close="closeDetail"
           />
        </v-card-text>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useDisplay } from 'vuetify';
import GenericList from '@/components/generic/GenericList.vue';
import GenericForm from '@/components/generic/GenericForm.vue';
import GenericDetail from '@/components/generic/GenericDetail.vue';
import CyberHeader from '@/components/shared/CyberHeader.vue';
import type { Company, FormSchema } from '@/types/entities';
import { companyService } from '@/services/companyService';
import { useAuthStore } from '@/stores/auth';
import { useContextStore } from '@/stores/context';

const { mobile } = useDisplay();
const authStore = useAuthStore();
const contextStore = useContextStore();
const items = ref<Company[]>([]);
const loading = ref(false);
const dialog = ref(false);
const detailDialog = ref(false);
const editedItem = ref<Company>({});
const detailItem = ref<Company>({});

const filteredItems = computed(() => {
    if (!contextStore.selectedPersonId) {
        return items.value;
    }
    const person = contextStore.people.find(p => p.id === contextStore.selectedPersonId);
    if (!person || !person.workExperiences) return [];

    const companyNames = new Set<string>();
    const companyIds = new Set<string>();

    person.workExperiences.forEach(we => {
        if (we.company) {
            // Check if company is string (legacy/simple) or object
            if (typeof we.company === 'string') {
                companyNames.add(we.company);
            } else {
                 if (we.company.id) companyIds.add(we.company.id);
                 if (we.company.name) companyNames.add(we.company.name);
            }
        }
    });

    return items.value.filter(c => {
        if (c.id && companyIds.has(c.id)) return true;
        if (c.name && companyNames.has(c.name)) return true;
        return false;
    });
});

const headers = computed(() => {
  return [
    { title: 'Name', key: 'name' },
    { title: 'Website', key: 'website' },
    { title: 'Desc', key: 'description' },
    { title: 'Actions', key: 'actions' }
  ];
});

const schema = ref<FormSchema>({
  fields: [
    { key: 'name', label: 'Name', type: 'text', required: true },
    { key: 'website', label: 'Website', type: 'text' },
    {
      key: 'description',
      label: 'Descriptions',
      type: 'object-array',
      itemSchema: {
        fields: [
          {
            key: 'language',
            label: 'Language',
            type: 'select',
            options: ['EN', 'IT', 'ES', 'DE', 'FR'],
            required: true
          },
          { key: 'value', label: 'Description', type: 'textarea', required: true },
        ]
      }
    }
  ]
});

const filterSchema = ref<FormSchema>({
  fields: [
    { key: 'name', label: 'Name', type: 'text' }
  ]
});

const loadData = async () => {
  loading.value = true;
  try {
    items.value = await companyService.getAll();
  } catch (e) {
    console.error('Error loading data', e);
  } finally {
    loading.value = false;
  }
};

onMounted(loadData);

const openDialog = (item?: Company) => {
  if (item) {
    editedItem.value = JSON.parse(JSON.stringify(item));
    if (!editedItem.value.description) {
        editedItem.value.description = [];
    }
  } else {
    editedItem.value = { description: [] };
  }
  dialog.value = true;
};

const closeDialog = () => {
  dialog.value = false;
};

const openDetail = (item: Company) => {
    detailItem.value = JSON.parse(JSON.stringify(item));
    detailDialog.value = true;
};

const closeDetail = () => {
    detailDialog.value = false;
};

const save = async (item: Company) => {
  try {
    if (item.id) {
      await companyService.update(item.id, item);
    } else {
      await companyService.create(item);
    }
    await loadData();
    closeDialog();
  } catch (e) {
    console.error('Save failed', e);
  }
};

const deleteItem = async (item: Company) => {
  if (confirm('Are you sure you want to delete this item?')) {
     try {
       await companyService.delete(item.id!);
       await loadData();
     } catch (e) {
       console.error('Delete failed', e);
     }
  }
};
</script>
