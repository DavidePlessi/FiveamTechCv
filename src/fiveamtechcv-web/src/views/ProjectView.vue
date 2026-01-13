<template>
  <div>
    <div class="d-flex justify-space-between align-center mb-4">
      <div class="d-flex align-center">
        <img src="@/assets/logo-nobg.png" alt="Logo" class="view-logo mr-4" />
        <h1 class="cyber-title">Projects</h1>
      </div>
      <v-btn color="primary" prepend-icon="mdi-plus" @click="openDialog()">Add Project</v-btn>
    </div>

    <generic-list
      :items="projects"
      :headers="headers"
      :loading="loading"
    >
      <template #item.description="{ item }">
        <div v-for="desc in item.description" :key="desc.language">
            <small><strong>{{ desc.language }}:</strong> {{ desc.value ? desc.value.substring(0, 50) + (desc.value.length > 50 ? '...' : '') : '' }}</small>
        </div>
      </template>

      <template #item.tags="{ item }">
        <v-chip
            v-for="tag in item.tags"
            :key="tag.id"
            size="small"
            class="mr-1 cyber-chip"
            color="secondary"
            variant="outlined"
            label
        >
            {{ tag.name }}
        </v-chip>
      </template>

      <template #item.actions="{ item }">
        <v-btn icon="mdi-pencil" size="small" variant="text" color="primary" @click="openDialog(item)"></v-btn>
        <v-btn icon="mdi-delete" size="small" variant="text" color="error" @click="deleteItem(item)"></v-btn>
      </template>
    </generic-list>

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
  </div>
</template>

<style scoped>
.view-logo {
    height: 100px;
    width: auto;
    filter: drop-shadow(0 0 5px rgba(255, 255, 255, 0.3));
}

.cyber-title {
    color: #fff;
    text-shadow: 0 0 10px rgba(255, 255, 255, 0.3);
    letter-spacing: 2px;
    margin-bottom: 0 !important;
}
</style>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useDisplay } from 'vuetify';
import GenericList from '@/components/generic/GenericList.vue';
import GenericForm from '@/components/generic/GenericForm.vue';
import type { Project, FormSchema } from '@/types/entities';
import { projectService } from '@/services/projectService';
import { tagService } from '@/services/tagService';

const { mobile } = useDisplay();
const projects = ref<Project[]>([]);
const loading = ref(false);
const dialog = ref(false);
const editedItem = ref<Project>({ name: '', description: [], tagIdsToLink: [] });
const tags = ref<any[]>([]);

const headers = [
  { title: 'Name', key: 'name' },
  { title: 'Order', key: 'order' },
  { title: 'Description', key: 'description' },
  { title: 'Tags', key: 'tags' },
  { title: 'Actions', key: 'actions', sortable: false },
];

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
          { key: 'language', label: 'Language', type: 'text', required: true },
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
    }
  ]
});

const loadData = async () => {
  loading.value = true;
  try {
    const [fetchedProjects, fetchedTags] = await Promise.all([
      projectService.getAll(),
      tagService.getAll()
    ]);
    projects.value = fetchedProjects;
    tags.value = fetchedTags.map(t => ({ text: t.name, value: t.id }));

    const tagField = projectSchema.value.fields.find(f => f.key === 'tagIdsToLink');
    if (tagField) {
        tagField.options = tags.value;
    }
  } catch (e) {
    console.error('Error loading data', e);
  } finally {
    loading.value = false;
  }
};

onMounted(loadData);

const openDialog = (item?: Project) => {
  if (item) {
    editedItem.value = JSON.parse(JSON.stringify(item));
    // Flatten tags for the form
    if (item.tags) {
        editedItem.value.tagIdsToLink = item.tags.map(t => t.id!);
        // Ensure description is initialized array if null
        if (!editedItem.value.description) editedItem.value.description = [];
    }
  } else {
    editedItem.value = { name: '', description: [], tagIdsToLink: [] };
  }
  dialog.value = true;
};

const closeDialog = () => {
  dialog.value = false;
};

const save = async (item: Project) => {
  try {
    if (item.id) {
       await projectService.update(item.id, item);
    } else {
       await projectService.create(item);
    }
    await loadData();
    closeDialog();
  } catch (e) {
    console.error('Save failed', e);
  }
};

const deleteItem = async (item: Project) => {
  if (confirm('Are you sure you want to delete this item?')) {
     try {
       await projectService.delete(item.id!);
       await loadData();
     } catch(e) {
        console.error('Delete failed', e);
     }
  }
};
</script>
