<template>
  <div class="dashboard-container">
    <cyber-header title="Dashboard" />

    <v-row>
      <!-- Stats Cards -->
      <v-col cols="12" md="4">
        <stat-card
            icon="mdi-tag-multiple"
            icon-color="primary"
            :value="totalTags"
            label="Total Tags"
        />
      </v-col>

      <v-col cols="12" md="4">
        <stat-card
            icon="mdi-rocket-launch"
            icon-color="secondary"
            :value="totalProjects"
            label="Total Projects"
        />
      </v-col>

      <v-col cols="12" md="4">
         <stat-card
            icon="mdi-chart-bell-curve-cumulative"
            icon-color="warning"
            :value="avgTagsPerProject"
            label="Avg Tags / Project"
        />
      </v-col>
    </v-row>

    <v-row class="mt-6">
      <!-- Tag Usage Monitor -->
      <v-col cols="12" md="8">
        <monitor-panel title="TAG_USAGE_MONITOR">
            <div v-if="loading" class="d-flex justify-center align-center" style="height: 300px;">
               <v-progress-circular indeterminate color="primary"></v-progress-circular>
            </div>
            <div v-else class="usage-list">
              <div v-for="tag in topTags" :key="tag.id" class="usage-item">
                <div class="usage-info">
                  <span class="tag-name">{{ tag.name }}</span>
                  <span class="tag-count">[{{ tag.projects?.length || 0 }}]</span>
                </div>
                <div class="usage-bar-bg">
                  <div
                    class="usage-bar-fill"
                    :style="{ width: getUsagePercentage(tag) + '%', backgroundColor: getBarColor(tag.type) }"
                  ></div>
                </div>
              </div>
            </div>
        </monitor-panel>
      </v-col>

      <!-- Tag Type Distribution -->
      <v-col cols="12" md="4">
         <monitor-panel title="TYPE_DISTRIBUTION">
             <div v-if="loading" class="d-flex justify-center align-center" style="height: 300px;">
               <v-progress-circular indeterminate color="primary"></v-progress-circular>
            </div>
            <div v-else class="type-list">
                <div v-for="(count, type) in tagTypeDistribution" :key="type" class="type-item">
                    <div class="d-flex justify-space-between mb-1">
                        <span class="type-name">{{ type }}</span>
                        <span class="type-count">{{ count }}</span>
                    </div>
                    <v-progress-linear
                        :model-value="(count / totalTags) * 100"
                        :color="getBarColor(TagType[type as keyof typeof TagType])"
                        height="6"
                        rounded
                    ></v-progress-linear>
                </div>
            </div>
        </monitor-panel>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { tagService } from '@/services/tagService';
import { projectService } from '@/services/projectService';
import type { Tag, Project } from '@/types/entities';
import { TagType } from '@/types/entities';
import CyberHeader from '@/components/shared/CyberHeader.vue';
import StatCard from '@/components/dashboard/StatCard.vue';
import MonitorPanel from '@/components/dashboard/MonitorPanel.vue';

const tags = ref<Tag[]>([]);
const projects = ref<Project[]>([]);
const loading = ref(true);

const totalTags = computed(() => tags.value.length);
const totalProjects = computed(() => projects.value.length);

const avgTagsPerProject = computed(() => {
  if (totalProjects.value === 0) return 0;
  const totalTagsLinked = projects.value.reduce((acc, curr) => acc + (curr.tags?.length || 0), 0);
  return (totalTagsLinked / totalProjects.value).toFixed(1);
});

const topTags = computed(() => {
  return [...tags.value]
    .sort((a, b) => (b.projects?.length || 0) - (a.projects?.length || 0))
    .slice(0, 10); // Top 10
});

const tagTypeDistribution = computed(() => {
    const distribution: Record<string, number> = {};
    tags.value.forEach(tag => {
        const typeName = TagType[tag.type!] || 'Unknown';
        distribution[typeName] = (distribution[typeName] || 0) + 1;
    });
    return distribution;
});

const getUsagePercentage = (tag: Tag) => {
  if (totalProjects.value === 0) return 0;
  return ((tag.projects?.length || 0) / totalProjects.value) * 100;
};

const getBarColor = (type?: TagType) => {
    switch (type) {
        case TagType.Language: return '#ff5f56'; // Red
        case TagType.Framework: return '#ffbd2e'; // Yellow
        case TagType.Database: return '#27c93f'; // Green
        case TagType.Platform: return '#00d1ff'; // Cyan
        default: return 'var(--accent-orange)';
    }
};

onMounted(async () => {
  loading.value = true;
  try {
    const [fetchedTags, fetchedProjects] = await Promise.all([
      tagService.getAll(),
      projectService.getAll()
    ]);
    tags.value = fetchedTags.filter(t => t.type !== TagType.Area && t.type !== TagType.Category);
    projects.value = fetchedProjects;
  } catch (e) {
    console.error('Failed to load dashboard data', e);
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.dashboard-container {
    font-family: var(--code-font, monospace);
}

/* Usage List */
.usage-item {
    margin-bottom: 15px;
}

.usage-info {
    display: flex;
    justify-content: space-between;
    margin-bottom: 5px;
    font-size: 0.9rem;
    color: #ccc;
}

.tag-count {
    color: var(--accent-pink);
}

.usage-bar-bg {
    height: 6px;
    background: #222;
    border: 1px solid #333;
    position: relative;
}

.usage-bar-fill {
    height: 100%;
    background: var(--accent-orange);
    box-shadow: 0 0 5px currentColor;
    transition: width 1s ease-out;
}

/* Type Distribution */
.type-item {
    margin-bottom: 20px;
}

.type-name {
    color: var(--text-dim);
    font-size: 0.85rem;
}

.type-count {
    color: #fff;
    font-weight: bold;
}
</style>
