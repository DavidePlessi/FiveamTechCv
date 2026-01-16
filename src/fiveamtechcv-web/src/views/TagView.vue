<template>
  <div>
    <cyber-header title="Tags" subtitle="Tags allow for flexible categorization of content across the application.">
    </cyber-header>

    <generic-list
      :items="filteredTags"
      :headers="headers"
      :loading="loading"
      :filter-schema="filterSchema"
      @detail="openDetail"
      @edit="openDialog"
      @delete="deleteItem"
    >
      <template #item.type="{ item }">
        {{ TagType[item.type] }}
      </template>

      <template #item.projects="{ item }">
        <cyber-chip
            v-for="project in item.projects"
            :key="project.id"
            :text="project.name"
            icon="mdi-rocket-launch-outline"
        />
      </template>

      <template #item.actions="{ item }">
        <v-btn icon="mdi-eye" size="small" variant="text" color="info" @click="openDetail(item)"></v-btn>
        <v-btn v-if="authStore.isAuthenticated" icon="mdi-pencil" size="small" variant="text" color="primary" @click="openDialog(item)"></v-btn>
        <v-btn v-if="authStore.isAuthenticated" icon="mdi-delete" size="small" variant="text" color="error" @click="deleteItem(item)"></v-btn>
      </template>
    </generic-list>

    <!-- Edit Dialog -->
    <v-dialog v-model="dialog" max-width="600px" :fullscreen="mobile" :transition="mobile ? 'dialog-bottom-transition' : 'dialog-transition'">
      <v-card>
        <v-toolbar v-if="mobile" color="primary" density="compact">
            <v-btn icon="mdi-close" @click="closeDialog"></v-btn>
            <v-toolbar-title>{{ editedItem.id ? 'Edit Tag' : 'New Tag' }}</v-toolbar-title>
        </v-toolbar>
        <v-card-title v-else>{{ editedItem.id ? 'Edit Tag' : 'New Tag' }}</v-card-title>
        <v-card-text>
          <generic-form
            v-if="dialog"
            :model-value="editedItem"
            :schema="tagSchema"
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
    <v-dialog v-model="detailDialog" max-width="600px" :fullscreen="mobile" :transition="mobile ? 'dialog-bottom-transition' : 'dialog-transition'">
      <v-card>
        <v-toolbar v-if="mobile" color="primary" density="compact">
            <v-btn icon="mdi-close" @click="closeDetail"></v-btn>
            <v-toolbar-title>Tag Details</v-toolbar-title>
        </v-toolbar>
        <v-card-title v-else>Tag Details</v-card-title>
        <v-card-text>
           <generic-detail
             v-if="detailDialog"
             :model-value="detailItem"
             :schema="tagSchema"
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
import type { Tag, FormSchema } from '@/types/entities';
import { TagType } from '@/types/entities';
import { tagService } from '@/services/tagService';
import { projectService } from '@/services/projectService';
import { useAuthStore } from '@/stores/auth';
import { useContextStore } from '@/stores/context';

const { mobile } = useDisplay();
const authStore = useAuthStore();
const contextStore = useContextStore();
const tags = ref<Tag[]>([]);
const loading = ref(false);
const dialog = ref(false);
const detailDialog = ref(false);
const editedItem = ref<Tag>({});
const detailItem = ref<Tag>({});
const projects = ref<any[]>([]); // These are lightweight for dropdown, but we need full projects for filter logic?
// actually loadData maps projects to {text, value}. We might need the full objects.
// Let's check loadData. It fetches projectService.getAll().
// But it assigns projects.value = fetchedProjects.map(...)
// We need to keep the full projects list to filter tags properly.

const allProjectsRaw = ref<any[]>([]);

const filteredTags = computed(() => {
    if (!contextStore.selectedPersonId) {
        return tags.value;
    }
    
    // 1. Find projects related to this person
    const personProjectIds = new Set<string>();
    allProjectsRaw.value.forEach(p => {
        if (p.people && p.people.some((per: any) => per.id === contextStore.selectedPersonId)) {
            personProjectIds.add(p.id);
        }
    });

    // 2. Filter tags that are linked to these projects
    // Note: The Tag entity on 'tags' list might have 'projects' populated (if backend sends it).
    // If not, we have to rely on the project's 'tags' list.
    // The TagView template uses item.projects, so tags.value likely has projects.
    
    return tags.value.filter(tag => {
        if (!tag.projects) return false;
        // Check if any of the tag's projects are in the person's project list
        return tag.projects.some(tp => personProjectIds.has(tp.id!));
    });
});

const headers = computed(() => {
  const baseHeaders = [
    { title: 'Name', key: 'name' },
    { title: 'Type', key: 'type' },
    { title: 'Order', key: 'order' },
    { title: 'Projects', key: 'projects' },
    { title: 'Actions', key: 'actions' }
  ];
  return baseHeaders;
});

const tagTypeOptions = Object.entries(TagType)
  .filter(([key, value]) => typeof value === 'number')
  .map(([key, value]) => ({ text: key, value: value }));

const tagSchema = ref<FormSchema>({
  fields: [
    { key: 'name', label: 'Name', type: 'text', required: true },
    {
      key: 'type',
      label: 'Type',
      type: 'select',
      options: tagTypeOptions,
      required: true
    },
    { key: 'order', label: 'Order', type: 'number' },
    { key: 'documentationLink', label: 'Documentation Link', type: 'text' },
    {
      key: 'projectIdsToLink',
      label: 'Related Projects',
      type: 'autocomplete',
      multiple: true,
      options: []
    }
  ]
});

const loadData = async () => {
  loading.value = true;
  try {
    const [fetchedTags, fetchedProjects] = await Promise.all([
      tagService.getAll(),
      projectService.getAll()
    ]);
    tags.value = fetchedTags;
    allProjectsRaw.value = fetchedProjects;
    projects.value = fetchedProjects.map(p => ({ text: p.name, value: p.id }));

    const projectField = tagSchema.value.fields.find(f => f.key === 'projectIdsToLink');
    if (projectField) {
        projectField.options = projects.value;
    }

    // Populate Filter Options
    const filterProjectField = filterSchema.value.fields.find(f => f.key === 'projects');
    if (filterProjectField) {
        filterProjectField.options = projects.value;
    }

    const filterTypeField = filterSchema.value.fields.find(f => f.key === 'type');
    if (filterTypeField) {
        const usedTypes = new Set(fetchedTags.map(t => t.type).filter(t => t !== undefined));
        filterTypeField.options = tagTypeOptions.filter(opt => usedTypes.has(opt.value as TagType));
    }
  } catch (e) {
    console.error('Error loading data', e);
  } finally {
    loading.value = false;
  }
};

const filterSchema = ref<FormSchema>({
  fields: [
    { key: 'name', label: 'Name', type: 'text' },
    { key: 'type', label: 'Type', type: 'select', options: [] },
    { key: 'projects', label: 'Project', type: 'select', options: [] }, // Options populated in loadData
  ]
});

onMounted(loadData);

const openDialog = (item?: Tag) => {
  if (item) {
    editedItem.value = JSON.parse(JSON.stringify(item));
    if (item.projects) {
        editedItem.value.projectIdsToLink = item.projects.map(p => p.id!);
    }
  } else {
    editedItem.value = {};
  }
  dialog.value = true;
};

const closeDialog = () => {
  dialog.value = false;
};

const openDetail = (item: Tag) => {
    detailItem.value = JSON.parse(JSON.stringify(item));
    if (item.projects) {
        detailItem.value.projectIdsToLink = item.projects.map(p => p.id!);
    }
    detailDialog.value = true;
};

const closeDetail = () => {
    detailDialog.value = false;
};

const save = async (item: Tag) => {
  try {
    if (item.id) {
      await tagService.update(item.id, item);
    } else {
      await tagService.create(item);
    }
    await loadData();
    closeDialog();
  } catch (e) {
    console.error('Save failed', e);
  }
};

const deleteItem = async (item: Tag) => {
  if (confirm('Are you sure you want to delete this item?')) {
     try {
       await tagService.delete(item.id!);
       await loadData();
     } catch (e) {
       console.error('Delete failed', e);
     }
  }
};
</script>
