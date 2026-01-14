<template>
  <div>
    <cyber-header title="Work Experiences">
        <v-btn v-if="authStore.isAuthenticated" color="primary" prepend-icon="mdi-plus" @click="openDialog()">Add Experience</v-btn>
    </cyber-header>

    <generic-list
      :items="items"
      :headers="headers"
      :loading="loading"
    >
      <template #item.dates="{ item }">
        {{ formatDate(item.startDate) }} - {{ item.endDate ? formatDate(item.endDate) : 'Present' }}
      </template>

      <template #item.projects="{ item }">
        <cyber-chip
            v-for="project in item.projects"
            :key="project.id"
            :text="project.name"
            icon="mdi-rocket-launch-outline"
        />
      </template>

      <template #item.tags="{ item }">
        <cyber-chip
            v-for="tag in item.tags"
            :key="tag.id"
            :text="tag.name"
            icon="mdi-tag-outline"
        />
      </template>

      <template #item.actions="{ item }">
        <v-btn v-if="authStore.isAuthenticated" icon="mdi-pencil" size="small" variant="text" color="primary" @click="openDialog(item)"></v-btn>
        <v-btn v-if="authStore.isAuthenticated" icon="mdi-delete" size="small" variant="text" color="error" @click="deleteItem(item)"></v-btn>
      </template>
    </generic-list>

    <v-dialog v-model="dialog" max-width="800px" :fullscreen="mobile" :transition="mobile ? 'dialog-bottom-transition' : 'dialog-transition'">
      <v-card>
        <v-toolbar v-if="mobile" color="primary" density="compact">
            <v-btn icon="mdi-close" @click="closeDialog"></v-btn>
            <v-toolbar-title>{{ editedItem.id ? 'Edit Experience' : 'New Experience' }}</v-toolbar-title>
        </v-toolbar>
        <v-card-title v-else>{{ editedItem.id ? 'Edit Experience' : 'New Experience' }}</v-card-title>
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
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useDisplay } from 'vuetify';
import GenericList from '@/components/generic/GenericList.vue';
import GenericForm from '@/components/generic/GenericForm.vue';
import CyberHeader from '@/components/shared/CyberHeader.vue';
import CyberChip from '@/components/shared/CyberChip.vue';
import type { WorkExperience, FormSchema } from '@/types/entities';
import { workExperienceService } from '@/services/workExperienceService';
import { projectService } from '@/services/projectService';
import { tagService } from '@/services/tagService';
import { useAuthStore } from '@/stores/auth';

const { mobile } = useDisplay();
const authStore = useAuthStore();
const items = ref<WorkExperience[]>([]);
const loading = ref(false);
const dialog = ref(false);
const editedItem = ref<WorkExperience>({});
const allProjects = ref<any[]>([]);
const allTags = ref<any[]>([]);

const headers = computed(() => {
  const baseHeaders = [
    { title: 'Order', key: 'order' },
    { title: 'Company', key: 'company' },
    { title: 'Position', key: 'position' },
    { title: 'Dates', key: 'dates' },
    { title: 'Projects', key: 'projects' },
    { title: 'Tags', key: 'tags' },
  ];
  if (authStore.isAuthenticated) {
    baseHeaders.push({ title: 'Actions', key: 'actions' });
  }
  return baseHeaders;
});

const schema = ref<FormSchema>({
  fields: [
    { key: 'company', label: 'Company', type: 'text', required: true },
    { key: 'position', label: 'Position', type: 'text', required: true },
    { key: 'startDate', label: 'Start Date', type: 'date', required: true },
    { key: 'endDate', label: 'End Date', type: 'date' },
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
    },
    {
      key: 'projectIdsToLink',
      label: 'Related Projects',
      type: 'autocomplete',
      multiple: true,
      options: []
    },
    {
      key: 'tagIdsToLink',
      label: 'Related Tags',
      type: 'autocomplete',
      multiple: true,
      options: []
    },
    { key: 'order', label: 'Order', type: 'number' },
  ]
});

const formatDate = (dateValue?: string) => {
    if (!dateValue) return '';
    // Use ISO string slice to get YYYY-MM-DD.
    // Assuming backend sends correct ISO format which parses correctly.
    try {
        return new Date(dateValue).toISOString().slice(0, 10);
    } catch (e) {
        return dateValue;
    }
};

const loadData = async () => {
  loading.value = true;
  try {
    const [fetchedItems, fetchedProjects, fetchedTags] = await Promise.all([
      workExperienceService.getAll(),
      projectService.getAll(),
      tagService.getAll()
    ]);
    items.value = fetchedItems;
    allProjects.value = fetchedProjects.map(p => ({ text: p.name, value: p.id }));
    allTags.value = fetchedTags.map(t => ({ text: t.name, value: t.id }));

    const projectField = schema.value.fields.find(f => f.key === 'projectIdsToLink');
    if (projectField) projectField.options = allProjects.value;

    const tagField = schema.value.fields.find(f => f.key === 'tagIdsToLink');
    if (tagField) tagField.options = allTags.value;

  } catch (e) {
    console.error('Error loading data', e);
  } finally {
    loading.value = false;
  }
};

onMounted(loadData);

const openDialog = (item?: WorkExperience) => {
  if (item) {
    editedItem.value = JSON.parse(JSON.stringify(item));
    // Flatten relationships for form
    if (item.projects) {
        editedItem.value.projectIdsToLink = item.projects.map(p => p.id!);
    }
    if (item.tags) {
        editedItem.value.tagIdsToLink = item.tags.map(t => t.id!);
    }
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

const save = async (item: WorkExperience) => {
  try {
    // Ensure dates are numbers if needed, or rely on form component to emit correct types.
    // DTO expects Long? StartDate. If generic form emits string date "YYYY-MM-DD", we might need conversion.
    // Usually generic form handles this or backend allows string parsing, but let's check.
    // DTO defines long? StartDate. Backend likely expects unix timestamp or ticks.
    // GenericForm type 'date' usually emits string "YYYY-MM-DD".
    // I should convert "YYYY-MM-DD" to timestamp before sending if necessary.
    // However, looking at TagView or ProjectView might clarify.
    // They don't have date fields.
    // Let's assume for now we need to convert to unix timestamp (seconds).

    // Payload date is already string (ISO "YYYY-MM-DD") from generic form datepicker
    const payload = { ...item };

    if (item.id) {
      await workExperienceService.update(item.id, payload);
    } else {
      await workExperienceService.create(payload);
    }
    await loadData();
    closeDialog();
  } catch (e) {
    console.error('Save failed', e);
  }
};

const deleteItem = async (item: WorkExperience) => {
  if (confirm('Are you sure you want to delete this item?')) {
     try {
       await workExperienceService.delete(item.id!);
       await loadData();
     } catch (e) {
       console.error('Delete failed', e);
     }
  }
};
</script>
