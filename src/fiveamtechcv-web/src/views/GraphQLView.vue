<template>
  <div class="d-flex flex-column h-100">
    <cyber-header title="GraphQL Playground" />

    <v-row class="flex-grow-1 ma-0 w-100">
      <v-col cols="12" md="6" class="d-flex flex-column pa-2" style="min-height: 50vh;">
        <v-card class="flex-grow-1 d-flex flex-column" elevation="4" border>
          <v-card-title class="d-flex align-center justify-space-between py-2 px-4 bg-surface-variant">
            <span class="text-caption font-weight-bold text-uppercase">Query</span>
            <div class="d-flex ga-2">
               <v-btn size="x-small" variant="tonal" @click="setExample('workExperiences')">Work Experiences</v-btn>
               <v-btn size="x-small" variant="tonal" @click="setExample('projects')">Projects</v-btn>
               <v-btn size="x-small" variant="tonal" @click="setExample('tags')">Tags</v-btn>
            </div>
          </v-card-title>

          <v-divider></v-divider>

          <v-card-text class="flex-grow-1 pa-0">
            <v-textarea
              v-model="query"
              variant="solo"
              hide-details
              class="code-editor h-100"
              no-resize
              spellcheck="false"
              placeholder="Write your GraphQL query here..."
              bg-color="#1e1e1e"
              color="white"
              theme="dark"
            ></v-textarea>
          </v-card-text>

          <v-divider></v-divider>

          <v-card-actions class="pa-3 bg-surface-variant">
            <v-spacer></v-spacer>
            <v-btn
              color="primary"
              variant="elevated"
              prepend-icon="mdi-play"
              @click="executeQuery"
              :loading="loading"
              class="px-6 font-weight-bold"
            >
              Execute
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>

      <v-col cols="12" md="6" class="d-flex flex-column pa-2" style="min-height: 50vh;">
        <v-card class="flex-grow-1 d-flex flex-column" elevation="4" border>
          <v-card-title class="py-2 px-4 bg-surface-variant">
            <span class="text-caption font-weight-bold text-uppercase">Result</span>
          </v-card-title>

          <v-divider></v-divider>

          <v-card-text class="flex-grow-1 pa-0 position-relative" style="background-color: #1e1e1e;">
             <div class="absolute-fill overflow-auto pa-4 custom-scrollbar">
                <pre class="code-font" :class="isError ? 'text-red-accent-2' : 'text-green-accent-3'">{{ results }}</pre>
             </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import httpClient from '@/services/httpClient';
import CyberHeader from '@/components/shared/CyberHeader.vue';

const query = ref(`query {
  projects(order: [{ order: ASC }]) {
    name
    description {
      language
      value
    }
    tags {
      name
      type
    }
  }
}`);

const results = ref('// Results will appear here...');
const loading = ref(false);
const isError = ref(false);

const examples: Record<string, string> = {
  projects: `query {
  projects(order: [{ order: ASC }]) {
    name
    description {
      language
      value
    }
    tags {
      name
      type
    }
  }
}`,
  workExperiences: `query {
  workExperiences(order: [{ order: ASC }]) {
    company
    position
    startDate
    endDate
    description {
      language
      value
    }
    projects {
      name
      description {
        language
        value
      }
    }
    tags {
      name
      type
    }
  }
}`,
  tags: `query {
  tags(order: [{ name: ASC }]) {
    name
    type
    projects {
      name,
      description {
        language
        value
      }
    }
  }
}`
};

const setExample = (type: string) => {
  if (examples[type]) {
    query.value = examples[type];
  }
};

const executeQuery = async () => {
  loading.value = true;
  isError.value = false;
  try {
    const response = await httpClient.post('/graphql', { query: query.value });
    results.value = JSON.stringify(response.data, null, 2);
  } catch (error: any) {
    isError.value = true;
    results.value = JSON.stringify(error.response ? error.response.data : error.message, null, 2);
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.code-editor :deep(.v-field__input) {
  font-family: 'Fira Code', monospace;
  font-size: 14px;
  line-height: 1.5;
  height: 100% !important;
}

.code-editor :deep(.v-field__field) {
    height: 100%;
}

.code-editor :deep(.v-input__control) {
    height: 100%;
}

.code-font {
  font-family: 'Fira Code', monospace;
  font-size: 14px;
  line-height: 1.5;
}

.absolute-fill {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}
</style>
