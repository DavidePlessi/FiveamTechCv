<template>
  <div>
    <cyber-header title="Work Experiences">
        <template #subtitle>
            Tags in Work Experience highlight specific <strong>Roles</strong> and skills.
        </template>
        <v-btn v-if="authStore.isAuthenticated" color="primary" prepend-icon="mdi-plus" @click="openDialog()">Add Experience</v-btn>
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
      <template #item.company="{ item }">
        <a v-if="item.company && item.company.website" :href="item.company.website" target="_blank" class="text-decoration-none text-high-emphasis font-weight-bold">
            {{ item.company.name }} <v-icon size="small" icon="mdi-open-in-new" color="primary"></v-icon>
        </a>
        <span v-else-if="item.company">{{ item.company.name }}</span>
      </template>

      <template #item.dates="{ item }">
        {{ formatDate(item.startDate) }} - {{ item.endDate ? formatDate(item.endDate) : 'Present' }}
      </template>

      <!-- <template #item.projects="{ item }">
        <cyber-chip
            v-for="project in item.projects"
            :key="project.id"
            :text="project.name"
            icon="mdi-rocket-launch-outline"
        />
      </template> -->

      <template #item.tags="{ item }">
        <cyber-chip
            v-for="tag in item.tags"
            :key="tag.id"
            :text="tag.name"
            icon="mdi-tag-outline"
        />
      </template>

      <template #item.person="{ item }">
        <cyber-chip
            v-if="item.person"
            :text="`${item.person.name} ${item.person.lastName}`"
            icon="mdi-account"
            color="info"
        />
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

    <!-- Detail Dialog -->
    <v-dialog v-model="detailDialog" max-width="800px" :fullscreen="mobile" :transition="mobile ? 'dialog-bottom-transition' : 'dialog-transition'">
      <v-card>
        <v-toolbar v-if="mobile" color="primary" density="compact">
            <v-btn icon="mdi-close" @click="closeDetail"></v-btn>
            <v-toolbar-title>Experience Details</v-toolbar-title>
        </v-toolbar>
        <v-card-title v-else>Experience Details</v-card-title>
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
import CyberChip from '@/components/shared/CyberChip.vue';
import type { WorkExperience, FormSchema } from '@/types/entities';
import { workExperienceService } from '@/services/workExperienceService';
import { projectService } from '@/services/projectService';
import { tagService } from '@/services/tagService';
import { personService } from '@/services/personService';
import { companyService } from '@/services/companyService';
import { useAuthStore } from '@/stores/auth';
import { useContextStore } from '@/stores/context';
import { formatDate } from '@/utils/date';

const { mobile } = useDisplay();
const authStore = useAuthStore();
const contextStore = useContextStore();
const items = ref<WorkExperience[]>([]);
const loading = ref(false);
const dialog = ref(false);
const detailDialog = ref(false);
const editedItem = ref<WorkExperience>({});
const detailItem = ref<WorkExperience>({});
const allProjects = ref<any[]>([]);
const allTags = ref<any[]>([]);
const allPeople = ref<any[]>([]);
const allCompanies = ref<any[]>([]);

const headers = computed(() => {
  const baseHeaders = [
    { title: 'Company', key: 'company' },
    { title: 'Person', key: 'person' },
    { title: 'Position', key: 'position' },
    { title: 'Dates', key: 'dates' },
    // { title: 'Projects', key: 'projects' },
    { title: 'Tags', key: 'tags' },
    { title: 'Actions', key: 'actions' }
  ];
  return baseHeaders;
});

const schema = ref<FormSchema>({
  fields: [
    {
      key: 'companyIdToLink',
      label: 'Company',
      type: 'autocomplete',
      multiple: false,
      options: [],
      required: true
    },
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
    {
      key: 'personIdToLink',
      label: 'Related Person',
      type: 'autocomplete',
      multiple: false,
      options: []
    },
    { key: 'order', label: 'Order', type: 'number' },
  ]
});

const filterSchema = ref<FormSchema>({
  fields: [
    { key: 'company', label: 'Company', type: 'text' },
    { key: 'position', label: 'Position', type: 'text' },
    { key: 'tags', label: 'Tag', type: 'select', options: [] },
    { key: 'people', label: 'Person', type: 'select', options: [] },
  ]
});



const loadData = async () => {
  loading.value = true;
  try {
    const [fetchedItems, fetchedProjects, fetchedTags, fetchedPeople, fetchedCompanies] = await Promise.all([
      workExperienceService.getAll(),
      projectService.getAll(),
      tagService.getAll(),
      personService.getAll(),
      companyService.getAll()
    ]);
    items.value = fetchedItems.map(item => ({
        ...item,
        title: `${item.position} at ${item.company?.name || 'Unknown'}`
    }));
    allProjects.value = fetchedProjects.map(p => ({ text: p.name, value: p.id }));
    allTags.value = fetchedTags.map(t => ({ text: t.name, value: t.id }));
    allPeople.value = fetchedPeople.map(p => ({ text: `${p.name} ${p.lastName}`, value: p.id }));
    allCompanies.value = fetchedCompanies.map(c => ({ text: c.name, value: c.id }));

    const companyField = schema.value.fields.find(f => f.key === 'companyIdToLink');
    if (companyField) companyField.options = allCompanies.value;

    const projectField = schema.value.fields.find(f => f.key === 'projectIdsToLink');
    if (projectField) projectField.options = allProjects.value;

    const tagField = schema.value.fields.find(f => f.key === 'tagIdsToLink');
    if (tagField) tagField.options = allTags.value;

    const personField = schema.value.fields.find(f => f.key === 'personIdToLink');
    if (personField) personField.options = allPeople.value;

    const filterTagField = filterSchema.value.fields.find(f => f.key === 'tags');
    if (filterTagField) {
        // Filter only used Experience Tags
        const usedTagIds = new Set<string>();
        items.value.forEach(item => {
            if (item.tags) {
                item.tags.forEach(t => {
                    if (t.id) usedTagIds.add(t.id);
                });
            }
        });

        filterTagField.options = allTags.value.filter(t => usedTagIds.has(t.value));
    }

    const filterPersonField = filterSchema.value.fields.find(f => f.key === 'people');
    if (filterPersonField) {
         const usedPersonIds = new Set<string>();
         items.value.forEach(item => {
             if (item.person && item.person.id) {
                 usedPersonIds.add(item.person.id);
             }
         });
         filterPersonField.options = allPeople.value.filter(p => usedPersonIds.has(p.value));
    }

  } catch (e) {
    console.error('Error loading data', e);
  } finally {
    loading.value = false;
  }
};

const filteredItems = computed(() => {
    if (!contextStore.selectedPersonId) {
        return items.value;
    }
    return items.value.filter(item => item.person && item.person.id === contextStore.selectedPersonId);
});

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
    if (item.person) {
        editedItem.value.personIdToLink = item.person.id;
    }
    if (item.company) {
        editedItem.value.companyIdToLink = item.company.id;
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

const openDetail = (item: WorkExperience) => {
    detailItem.value = JSON.parse(JSON.stringify(item));
    if (item.projects) {
        detailItem.value.projectIdsToLink = item.projects.map(p => p.id!);
    }
    if (item.tags) {
        detailItem.value.tagIdsToLink = item.tags.map(t => t.id!);
    }
    if (item.person) {
        detailItem.value.personIdToLink = item.person.id; // Or setup display property if detail view uses schema to resolve ID to Text
    }
    if (item.company) {
        detailItem.value.companyIdToLink = item.company.id;
    }
    detailDialog.value = true;
};

const closeDetail = () => {
    detailDialog.value = false;
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
