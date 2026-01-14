<template>
  <div class="dashboard-container">
    <div class="d-flex align-center mb-6">
      <img src="@/assets/logo-nobg.png" alt="Logo" class="view-logo mr-4" />
      <h1 class="cyber-title">Dashboard</h1>
    </div>

    <v-row>
      <!-- Stats Cards -->
      <v-col cols="12" md="4">
        <div class="stat-card">
          <div class="stat-icon">
            <v-icon size="large" color="primary">mdi-tag-multiple</v-icon>
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ totalTags }}</div>
            <div class="stat-label">Total Tags</div>
          </div>
          <div class="corner-accent top-right"></div>
          <div class="corner-accent bottom-left"></div>
        </div>
      </v-col>

      <v-col cols="12" md="4">
        <div class="stat-card">
          <div class="stat-icon">
            <v-icon size="large" color="secondary">mdi-rocket-launch</v-icon>
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ totalProjects }}</div>
            <div class="stat-label">Total Projects</div>
          </div>
          <div class="corner-accent top-right"></div>
          <div class="corner-accent bottom-left"></div>
        </div>
      </v-col>

      <v-col cols="12" md="4">
        <div class="stat-card">
          <div class="stat-icon">
            <v-icon size="large" color="warning">mdi-chart-bell-curve-cumulative</v-icon>
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ avgTagsPerProject }}</div>
            <div class="stat-label">Avg Tags / Project</div>
          </div>
          <div class="corner-accent top-right"></div>
          <div class="corner-accent bottom-left"></div>
        </div>
      </v-col>
    </v-row>

    <v-row class="mt-6">
      <!-- Tag Usage Monitor -->
      <v-col cols="12" md="8">
        <div class="monitor-panel">
          <div class="panel-header">
            <span class="blink-cursor">_</span> TAG_USAGE_MONITOR
          </div>
          <div class="panel-body">
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
          </div>
        </div>
      </v-col>

      <!-- Tag Type Distribution -->
      <v-col cols="12" md="4">
         <div class="monitor-panel">
          <div class="panel-header">
            <span class="blink-cursor">_</span> TYPE_DISTRIBUTION
          </div>
          <div class="panel-body">
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
          </div>
        </div>
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

.view-logo {
    height: 100px;
    width: auto;
    filter: drop-shadow(0 0 5px rgba(255, 255, 255, 0.3));
}

.cyber-title {
    color: #fff;
    text-shadow: 0 0 10px rgba(255, 255, 255, 0.3);
    letter-spacing: 2px;
    margin-bottom: 0 !important; /* Override mb-6 from previous if needed, handled by flex container margin */
}

/* Stat Cards */
.stat-card {
    background: rgba(20, 20, 25, 0.6);
    border: 1px solid rgba(255, 255, 255, 0.1);
    padding: 20px;
    display: flex;
    align-items: center;
    gap: 20px;
    position: relative;
    overflow: hidden;
    backdrop-filter: blur(5px);
}

.stat-icon {
    background: rgba(255, 255, 255, 0.05);
    padding: 15px;
    border-radius: 50%;
    border: 1px solid rgba(255, 255, 255, 0.1);
}

.stat-value {
    font-size: 2rem;
    font-weight: bold;
    color: #fff;
    line-height: 1;
}

.stat-label {
    color: var(--text-dim);
    font-size: 0.9rem;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.corner-accent {
    position: absolute;
    width: 10px;
    height: 10px;
    border: 2px solid var(--accent-orange);
    opacity: 0.5;
}

.top-right { top: 0; right: 0; border-bottom: none; border-left: none; }
.bottom-left { bottom: 0; left: 0; border-top: none; border-right: none; }

/* Monitor Panel */
.monitor-panel {
    background: #0a0a0f;
    border: 1px solid #333;
    border-top: 2px solid var(--accent-orange);
    border-radius: 4px;
    overflow: hidden;
    height: 100%;
    min-height: 400px;
    display: flex;
    flex-direction: column;
}

.panel-header {
    background: #111;
    padding: 10px 15px;
    font-size: 0.8rem;
    color: var(--accent-orange);
    border-bottom: 1px solid #333;
    font-weight: bold;
    letter-spacing: 1px;
}

.panel-body {
    padding: 20px;
    flex-grow: 1;
    overflow-y: auto;
    background: repeating-linear-gradient(
        0deg,
        transparent,
        transparent 19px,
        rgba(255, 255, 255, 0.02) 20px
    );
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

.blink-cursor {
    animation: blink 1s infinite;
}

@keyframes blink {
    0%, 100% { opacity: 1; }
    50% { opacity: 0; }
}
</style>
