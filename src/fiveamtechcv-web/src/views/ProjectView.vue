<template>
  <div>
    <cyber-header title="Projects">
        <template #subtitle>
            Tags in Projects indicate the <strong>Technology</strong> stack and <strong>Category</strong>.
        </template>
        <v-btn v-if="authStore.isAuthenticated" color="primary" prepend-icon="mdi-plus" @click="openDialog()">Add Project</v-btn>
    </cyber-header>

    <generic-list
      :items="filteredProjects"
      :headers="headers"
      :loading="loading"
      :filter-schema="filterSchema"
      @detail="openDetail"
      @edit="openDialog"
      @delete="deleteItem"
    >
      <template #item.description="{ item }">
        <div v-for="desc in item.description" :key="desc.language">
            <small><strong>{{ desc.language }}:</strong> {{ desc.value ? desc.value.substring(0, 50) + (desc.value.length > 50 ? '...' : '') : '' }}</small>
        </div>
      </template>

      <template #item.tags="{ item }">
        <cyber-chip
            v-for="tag in item.tags"
            :key="tag.id"
            :text="tag.name"
        />
      </template>

      <template #item.people="{ item }">
        <cyber-chip
            v-for="person in item.people"
            :key="person.id"
            :text="`${person.name} ${person.lastName}`"
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
            <v-toolbar-title>{{ editedItem.id ? 'Edit Project' : 'New Project' }}</v-toolbar-title>
        </v-toolbar>
        <v-card-title v-else>{{ editedItem.id ? 'Edit Project' : 'New Project' }}</v-card-title>
        <v-card-text>
          <generic-form
            v-if="dialog"
            :model-value="editedItem"
            :schema="projectSchema"
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
            <v-toolbar-title>Project Details</v-toolbar-title>
        </v-toolbar>
        <v-card-title v-else>Project Details</v-card-title>
        <v-card-text>
           <generic-detail
             v-if="detailDialog"
             :model-value="detailItem"
             :schema="projectSchema"
             @close="closeDetail"
           />
        </v-card-text>
      </v-card>
    </v-dialog>
  </div>
</template>

<style scoped>
/* Scoped styles removed in favor of global cyber.css and shared components */
</style>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useDisplay } from 'vuetify';
import GenericList from '@/components/generic/GenericList.vue';
import GenericForm from '@/components/generic/GenericForm.vue';
import GenericDetail from '@/components/generic/GenericDetail.vue';
import CyberHeader from '@/components/shared/CyberHeader.vue';
import CyberChip from '@/components/shared/CyberChip.vue';
import type { Project, FormSchema } from '@/types/entities';
import { projectService } from '@/services/projectService';
import { tagService } from '@/services/tagService';
import { personService } from '@/services/personService';
import { useAuthStore } from '@/stores/auth';
import { useContextStore } from '@/stores/context';

const { mobile } = useDisplay();
const authStore = useAuthStore();
const contextStore = useContextStore();
const projects = ref<Project[]>([]);
const loading = ref(false);
const dialog = ref(false);
const detailDialog = ref(false);
const editedItem = ref<Project>({ name: '', description: [], tagIdsToLink: [], personIdsToLink: [] });
const detailItem = ref<Project>({ name: '', description: [], tagIdsToLink: [], personIdsToLink: [] });
const tags = ref<any[]>([]);
const people = ref<any[]>([]);

const filteredProjects = computed(() => {
    if (!contextStore.selectedPersonId) {
        return projects.value;
    }
    return projects.value.filter(p => p.people && p.people.some(per => per.id === contextStore.selectedPersonId));
});

const headers = computed(() => {
  const baseHeaders = [
    { title: 'Name', key: 'name' },
    { title: 'Order', key: 'order' },
    { title: 'Description', key: 'description' },
    { title: 'Tags', key: 'tags' },
    { title: 'People', key: 'people' },
    { title: 'Actions', key: 'actions' }
  ];
  return baseHeaders;
});

const projectSchema = ref<FormSchema>({
  fields: [
    { key: 'name', label: 'Name', type: 'text', required: true },
    { key: 'order', label: 'Order', type: 'number' },
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
      key: 'tagIdsToLink',
      label: 'Tags',
      type: 'autocomplete',
      multiple: true,
      options: []
    },
    {
      key: 'personIdsToLink',
      label: 'People',
      type: 'autocomplete',
      multiple: true,
      options: []
    }
  ]
});

const filterSchema = ref<FormSchema>({
  fields: [
    { key: 'name', label: 'Name', type: 'text' },
    { key: 'tags', label: 'Tag', type: 'select', options: [] }, // Options populated in loadData
    { key: 'people', label: 'Person', type: 'select', options: [] },
  ]
});

onMounted(async () => {
    loading.value = true;
    try {
        const [fetchedProjects, fetchedTags, fetchedPeople] = await Promise.all([
            projectService.getAll(),
            tagService.getAll(),
            personService.getAll()
        ]);
        projects.value = fetchedProjects;
        tags.value = fetchedTags.map(t => ({ text: t.name, value: t.id }));
        people.value = fetchedPeople.map(p => ({ text: `${p.name} ${p.lastName}`, value: p.id}));

        const tagField = projectSchema.value.fields.find(f => f.key === 'tagIdsToLink');
        if (tagField) {
            tagField.options = tags.value;
        }

        const personField = projectSchema.value.fields.find(f => f.key === 'personIdsToLink');
        if (personField) {
            personField.options = people.value;
        }

        // Populate Filter Options
        const filterTagField = filterSchema.value.fields.find(f => f.key === 'tags');
        if (filterTagField) {
            // Filter only used Project Tags
            const usedTagIds = new Set<string>();
            projects.value.forEach(p => {
                if (p.tags) {
                    p.tags.forEach(t => {
                        if (t.id) usedTagIds.add(t.id);
                    });
                }
            });
            filterTagField.options = tags.value.filter(t => usedTagIds.has(t.value));
        }

        const filterPersonField = filterSchema.value.fields.find(f => f.key === 'people');
        if (filterPersonField) {
             const usedPersonIds = new Set<string>();
             projects.value.forEach(p => {
                 if (p.people) {
                     p.people.forEach(per => {
                         if (per.id) usedPersonIds.add(per.id);
                     });
                 }
             });
             filterPersonField.options = people.value.filter(p => usedPersonIds.has(p.value));
        }
    } catch (e) {
        console.error('Error loading data', e);
    } finally {
        loading.value = false;
    }
});

const openDialog = (item?: Project) => {
  if (item) {
    editedItem.value = JSON.parse(JSON.stringify(item));
    // Flatten tags for the form
    if (item.tags) {
        editedItem.value.tagIdsToLink = item.tags.map(t => t.id!);
        // Ensure description is initialized array if null
        if (!editedItem.value.description) editedItem.value.description = [];
    }
    if (item.people) {
        editedItem.value.personIdsToLink = item.people.map(p => p.id!);
        if(!editedItem.value.description) editedItem.value.description = [];
    }
  } else {
    editedItem.value = { name: '', description: [], tagIdsToLink: [], personIdsToLink: [] };
  }
  dialog.value = true;
};

const closeDialog = () => {
  dialog.value = false;
};

const openDetail = (item: Project) => {
    detailItem.value = JSON.parse(JSON.stringify(item));
    if (item.tags) {
        detailItem.value.tagIdsToLink = item.tags.map(t => t.id!);
    }
    if (item.people) {
        detailItem.value.personIdsToLink = item.people.map(p => p.id!);
    }
    detailDialog.value = true;
};

const closeDetail = () => {
    detailDialog.value = false;
};

const save = async (item: Project) => {
  try {
    if (item.id) {
       await projectService.update(item.id, item);
    } else {
       await projectService.create(item);
    }
    await loadDataSafe(); // using separate function to avoid recursion or confusion
    closeDialog();
  } catch (e) {
    console.error('Save failed', e);
  }
};

const deleteItem = async (item: Project) => {
  if (confirm('Are you sure you want to delete this item?')) {
     try {
       await projectService.delete(item.id!);
       await loadDataSafe();
     } catch(e) {
        console.error('Delete failed', e);
     }
  }
};

const loadDataSafe = async () => {
    loading.value = true;
    try {
        const fetchedProjects = await projectService.getAll();
        projects.value = fetchedProjects;
    } catch (e) {
        console.error('Error reloading projects', e);
    } finally {
        loading.value = false;
    }
}
</script>
