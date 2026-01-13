<template>
  <div>
    <div class="d-flex justify-space-between align-center mb-4">
      <div class="d-flex align-center">
        <img src="@/assets/logo-nobg.png" alt="Logo" class="view-logo mr-4" />
        <h1 class="cyber-title">Tags</h1>
      </div>
      <v-btn color="primary" prepend-icon="mdi-plus" @click="openDialog()">Add Tag</v-btn>
    </div>

    <generic-list
      :items="tags"
      :headers="headers"
      :loading="loading"
    >
      <template #item.type="{ item }">
        {{ TagType[item.type] }}
      </template>

      <template #item.projects="{ item }">
        <v-chip
          v-for="project in item.projects"
          :key="project.id"
          size="small"
          class="mr-1 mb-1 cyber-chip"
          color="primary"
          variant="outlined"
          label
        >
            <v-icon start size="x-small">mdi-rocket-launch-outline</v-icon>
            {{ project.name }}
        </v-chip>
      </template>

      <template #item.actions="{ item }">
        <v-btn icon="mdi-pencil" size="small" variant="text" color="primary" @click="openDialog(item)"></v-btn>
        <v-btn icon="mdi-delete" size="small" variant="text" color="error" @click="deleteItem(item)"></v-btn>
      </template>
    </generic-list>

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
import type { Tag, FormSchema } from '@/types/entities';
import { TagType } from '@/types/entities';
import { tagService } from '@/services/tagService';
import { projectService } from '@/services/projectService';

const { mobile } = useDisplay();
const tags = ref<Tag[]>([]);
const loading = ref(false);
const dialog = ref(false);
const editedItem = ref<Tag>({});
const projects = ref<any[]>([]);

const headers = [
  { title: 'Name', key: 'name' },
  { title: 'Type', key: 'type' },
  { title: 'Order', key: 'order' },
  { title: 'Projects', key: 'projects' },
  { title: 'Actions', key: 'actions', sortable: false },
];

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
    projects.value = fetchedProjects.map(p => ({ text: p.name, value: p.id }));

    const projectField = tagSchema.value.fields.find(f => f.key === 'projectIdsToLink');
    if (projectField) {
        projectField.options = projects.value;
    }
  } catch (e) {
    console.error('Error loading data', e);
  } finally {
    loading.value = false;
  }
};

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
