<template>
  <div>
    <cyber-header title="People">
        <template #subtitle>
            Manage the <strong>People</strong> entities in the system.
        </template>
        <v-btn v-if="authStore.isAuthenticated" color="primary" prepend-icon="mdi-plus" @click="openDialog()">Add Person</v-btn>
    </cyber-header>

    <generic-list
      :items="people"
      :headers="headers"
      :loading="loading"
      :filter-schema="filterSchema"
      @detail="openDetail"
      @edit="openDialog"
      @delete="deleteItem"
    >
        <template #item.info="{ item }">
             <div v-for="desc in item.info" :key="desc.language">
                 <small><strong>{{ desc.language }}:</strong> {{ desc.value ? desc.value.substring(0, 50) + (desc.value.length > 50 ? '...' : '') : '' }}</small>
             </div>
        </template>
        <template #item.bornDate="{ item }">
            {{ formatDate(item.bornDate) }}
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
            <v-toolbar-title>{{ editedItem.id ? 'Edit Person' : 'New Person' }}</v-toolbar-title>
        </v-toolbar>
        <v-card-title v-else>{{ editedItem.id ? 'Edit Person' : 'New Person' }}</v-card-title>
        <v-card-text>
          <generic-form
            v-if="dialog"
            :model-value="editedItem"
            :schema="personSchema"
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
            <v-toolbar-title>Person Details</v-toolbar-title>
        </v-toolbar>
        <v-card-title v-else>Person Details</v-card-title>
        <v-card-text>
           <generic-detail
             v-if="detailDialog"
             :model-value="detailItem"
             :schema="personSchema"
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
import type { Person, FormSchema, Project, Tag, WorkExperience } from '@/types/entities';
import { personService } from '@/services/personService';
import { projectService } from '@/services/projectService';
import { tagService } from '@/services/tagService';
import { workExperienceService } from '@/services/workExperienceService';
import { useAuthStore } from '@/stores/auth';
import { formatDate } from '@/utils/date';

const { mobile } = useDisplay();
const authStore = useAuthStore();
const people = ref<Person[]>([]);
const loading = ref(false);
const dialog = ref(false);
const detailDialog = ref(false);
const editedItem = ref<Person>({});
const detailItem = ref<Person>({});

// Select Options
const projects = ref<any[]>([]);
const tags = ref<any[]>([]);
const workExperiences = ref<any[]>([]);

const headers = computed(() => {
  return [
    { title: 'Name', key: 'name' },
    { title: 'Last Name', key: 'lastName' },
    { title: 'Born Date', key: 'bornDate' },
    { title: 'Info', key: 'info' },
    { title: 'Actions', key: 'actions' }
  ];
});


const localizedStringSchema: any = {
    fields: [
        {
            key: 'language',
            label: 'Language',
            type: 'select',
            options: ['EN', 'IT', 'ES', 'DE', 'FR'],
            required: true
        },
        { key: 'value', label: 'Value', type: 'textarea', required: true },
    ]
};

const personSchema = ref<FormSchema>({
  fields: [
    { key: 'name', label: 'Name', type: 'text', required: true },
    { key: 'lastName', label: 'Last Name', type: 'text', required: true },
    { key: 'bornDate', label: 'Born Date', type: 'date' },
    {
      key: 'info',
      label: 'Info',
      type: 'object-array',
      itemSchema: localizedStringSchema
    },
    {
      key: 'summary',
      label: 'Summary',
      type: 'object-array',
      itemSchema: localizedStringSchema
    },
    {
      key: 'mindset',
      label: 'Mindset',
      type: 'object-array',
      itemSchema: localizedStringSchema
    },
    {
      key: 'slogan',
      label: 'Slogan',
      type: 'object-array',
      itemSchema: localizedStringSchema
    },
    {
      key: 'projectIdsToLink',
      label: 'Projects',
      type: 'autocomplete',
      multiple: true,
      options: []
    },
    {
      key: 'workExperienceIdsToLink',
      label: 'Work Experiences',
      type: 'autocomplete',
      multiple: true,
      options: []
    },
    {
      key: 'tagIdsToLink',
      label: 'Tags',
      type: 'autocomplete',
      multiple: true,
      options: []
    }
  ]
});

const filterSchema = ref<FormSchema>({
  fields: [
    { key: 'name', label: 'Name', type: 'text' },
    { key: 'lastName', label: 'Last Name', type: 'text' },
  ]
});

onMounted(async () => {
    loading.value = true;
    try {
        const [fetchedPeople, fetchedProjects, fetchedTags, fetchedWorkExperiences] = await Promise.all([
            personService.getAll(),
            projectService.getAll(),
            tagService.getAll(),
            workExperienceService.getAll()
        ]);
        people.value = fetchedPeople;
        
        projects.value = fetchedProjects.map(p => ({ text: p.name, value: p.id }));
        tags.value = fetchedTags.map(t => ({ text: t.name, value: t.id }));
        workExperiences.value = fetchedWorkExperiences.map(w => ({ text: `${w.company} - ${w.position}`, value: w.id }));

        // Update Schema Options
        const projectField = personSchema.value.fields.find(f => f.key === 'projectIdsToLink');
        if (projectField) projectField.options = projects.value;
        
        const tagField = personSchema.value.fields.find(f => f.key === 'tagIdsToLink');
        if (tagField) tagField.options = tags.value;
        
        const workExpField = personSchema.value.fields.find(f => f.key === 'workExperienceIdsToLink');
        if (workExpField) workExpField.options = workExperiences.value;

    } catch (e) {
        console.error('Error loading data', e);
    } finally {
        loading.value = false;
    }
});

const openDialog = (item?: Person) => {
  if (item) {
    editedItem.value = JSON.parse(JSON.stringify(item));
    // Flatten relations for the form
    if (item.projects) editedItem.value.projectIdsToLink = item.projects.map(p => p.id!);
    if (item.tags) editedItem.value.tagIdsToLink = item.tags.map(t => t.id!);
    if (item.workExperiences) editedItem.value.workExperienceIdsToLink = item.workExperiences.map(w => w.id!);
    
    // Ensure arrays are initialized
    if (!editedItem.value.info) editedItem.value.info = [];
    if (!editedItem.value.summary) editedItem.value.summary = [];
    if (!editedItem.value.mindset) editedItem.value.mindset = [];
    if (!editedItem.value.slogan) editedItem.value.slogan = [];

  } else {
    editedItem.value = { 
        info: [], summary: [], mindset: [], slogan: [], 
        projectIdsToLink: [], tagIdsToLink: [], workExperienceIdsToLink: [] 
    };
  }
  dialog.value = true;
};

const closeDialog = () => {
  dialog.value = false;
};

const openDetail = (item: Person) => {
    detailItem.value = JSON.parse(JSON.stringify(item));
    
    if (item.projects) detailItem.value.projectIdsToLink = item.projects.map(p => p.id!);
    if (item.workExperiences) detailItem.value.workExperienceIdsToLink = item.workExperiences.map(w => w.id!);
    if (item.tags) detailItem.value.tagIdsToLink = item.tags.map(t => t.id!);
    
    detailDialog.value = true;
};

const closeDetail = () => {
    detailDialog.value = false;
};

const save = async (item: Person) => {
  try {
    if (item.id) {
       await personService.update(item.id, item);
    } else {
       await personService.create(item);
    }
    await loadDataSafe();
    closeDialog();
  } catch (e) {
    console.error('Save failed', e);
  }
};

const deleteItem = async (item: Person) => {
  if (confirm('Are you sure you want to delete this item?')) {
     try {
       await personService.delete(item.id!);
       await loadDataSafe();
     } catch(e) {
        console.error('Delete failed', e);
     }
  }
};

const loadDataSafe = async () => {
    loading.value = true;
    try {
        people.value = await personService.getAll();
    } catch (e) {
        console.error('Error reloading people', e);
    } finally {
        loading.value = false;
    }
}
</script>
