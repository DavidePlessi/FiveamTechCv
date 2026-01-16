<template>
  <div class="dashboard-container">
    <cyber-header title="Dashboard">
        <v-btn
            prepend-icon="mdi-console"
            color="#0f0"
            variant="outlined"
            class="stats-card-decoration"
            style="border-color: #0f0; color: #0f0;"
            @click="isConsoleOpen = true"
        >
            AI TERMINAL
        </v-btn>
    </cyber-header>

    <ai-console :is-open="isConsoleOpen" @close="isConsoleOpen = false" />

    <v-row>
      <!-- Stats Cards -->
      <v-col cols="12" md="4">
        <stat-card
            icon="mdi-tag-multiple"
            icon-color="primary"
            :value="totalTags"
            label="Total Technologies"
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
            label="Avg Tech. / Project"
        />
      </v-col>
    </v-row>

    <!-- Work Experience Stats -->
    <v-row>
        <v-col cols="12" md="4">
            <stat-card
                icon="mdi-briefcase"
                icon-color="info"
                :value="totalCompanies"
                label="Total Companies"
            />
        </v-col>
        <v-col cols="12" md="4">
             <stat-card
                icon="mdi-clock-time-four-outline"
                icon-color="success"
                :value="totalYearsExperience"
                label="Total Years Exp."
            />
        </v-col>
        <v-col cols="12" md="4">
             <stat-card
                icon="mdi-domain"
                icon-color="purple"
                :value="avgCompanyTenure"
                label="Avg Time / Company"
            />
        </v-col>
    </v-row>

    <v-row class="mt-6">
      <!-- Tag Usage Monitor -->
      <v-col cols="12" md="8">
        <monitor-panel title="TOP_TECH_USAGE_MONITOR">
            <div v-if="loading" class="d-flex justify-center align-center" style="height: 300px;">
               <v-progress-circular indeterminate color="primary"></v-progress-circular>
            </div>
            <div v-else class="usage-list">
              <div v-for="tag in topTags" :key="tag.id" class="usage-item">
                <div class="usage-info">
                  <span class="tag-name">{{ tag.name }}</span>
                  <span class="tag-count">[{{ tag.projects?.length || 0 }}/{{ totalProjects }}]</span>
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

      <!-- Category Duration Distribution -->
      <v-col cols="12" md="4">
        <monitor-panel title="WORK_CATEGORY_DISTRIBUTION">
          <div v-if="loading" class="d-flex justify-center align-center" style="height: 300px;">
            <v-progress-circular indeterminate color="primary"></v-progress-circular>
          </div>
          <div v-else class="type-list">
            <div v-for="(months, category) in categoryDurationDistribution" :key="category" class="type-item">
              <div class="d-flex justify-space-between mb-1">
                <span class="type-name">{{ category }}</span>
                <span class="type-count">{{ formatDuration(months) }}</span>
              </div>
              <v-progress-linear
                :model-value="(months / totalCategoryMonths) * 100"
                :color="getBarColor(TagType.Category)"
                height="6"
                rounded
              ></v-progress-linear>
            </div>
          </div>
        </monitor-panel>

      </v-col>
    </v-row>

<!--    <v-row>-->
<!--        <v-col cols="12">-->
<!--          &lt;!&ndash; Tag Type Distribution &ndash;&gt;-->
<!--          <monitor-panel title="TYPE_DISTRIBUTION">-->
<!--            <div v-if="loading" class="d-flex justify-center align-center" style="height: 300px;">-->
<!--              <v-progress-circular indeterminate color="primary"></v-progress-circular>-->
<!--            </div>-->
<!--            <div v-else class="type-list">-->
<!--              <div v-for="(count, type) in tagTypeDistribution" :key="type" class="type-item">-->
<!--                <div class="d-flex justify-space-between mb-1">-->
<!--                  <span class="type-name">{{ type }}</span>-->
<!--                  <span class="type-count">{{ count }}</span>-->
<!--                </div>-->
<!--                <v-progress-linear-->
<!--                  :model-value="(count / totalTags) * 100"-->
<!--                  :color="getBarColor(TagType[type as keyof typeof TagType])"-->
<!--                  height="6"-->
<!--                  rounded-->
<!--                ></v-progress-linear>-->
<!--              </div>-->
<!--            </div>-->
<!--          </monitor-panel>-->
<!--        </v-col>-->
<!--    </v-row>-->
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { tagService } from '@/services/tagService';
import { projectService } from '@/services/projectService';
import { workExperienceService } from '@/services/workExperienceService';
import type { Tag, Project, WorkExperience } from '@/types/entities';
import { TagType } from '@/types/entities';
import CyberHeader from '@/components/shared/CyberHeader.vue';
import StatCard from '@/components/dashboard/StatCard.vue';
import MonitorPanel from '@/components/dashboard/MonitorPanel.vue';
import AiConsole from '@/components/AiConsole.vue';

const tags = ref<Tag[]>([]);
const projects = ref<Project[]>([]);
const workExperiences = ref<WorkExperience[]>([]);
const loading = ref(true);
const isConsoleOpen = ref(false);

const totalTags = computed(() => tags.value.length);
const totalProjects = computed(() => projects.value.length);

const avgTagsPerProject = computed(() => {
  if (totalProjects.value === 0) return 0;
  const totalTagsLinked = projects.value.reduce((acc, curr) => acc + (curr.tags?.length || 0), 0);
  return (totalTagsLinked / totalProjects.value).toFixed(1);
});

// Work Experience Stats
const totalCompanies = computed(() => {
    const companies = new Set(workExperiences.value.map(w => w.company?.name?.trim()).filter(Boolean));
    return companies.size;
});

const calculateDurationInMonths = (start?: string, end?: string) => {
    if (!start) return 0;
    const startDate = new Date(start);
    const endDate = end ? new Date(end) : new Date();

    // Calculate difference in months
    const years = endDate.getFullYear() - startDate.getFullYear();
    const months = endDate.getMonth() - startDate.getMonth();
    return (years * 12) + months; // approximate
};

// Helper to calculate merged duration from intervals
const calculateMergedDuration = (intervals: { start: number; end: number }[]) => {
    if (intervals.length === 0) return 0;

    // Sort intervals
    intervals.sort((a, b) => a.start - b.start);

    // Merge overlapping intervals
    const merged: { start: number, end: number }[] = [];

    // Safety check for first element
    if (intervals[0]) {
        let current = intervals[0];

        for (let i = 1; i < intervals.length; i++) {
            const next = intervals[i];
            if (next && current && next.start <= current.end) {
                // Overlap or adjacent, extend end if needed
                current.end = Math.max(current.end, next.end);
            } else if (next && current) {
                // No overlap, push current and start new
                merged.push(current);
                current = next;
            }
        }
        if (current) merged.push(current);
    }

    // Sum durations
    let totalMs = 0;
    merged.forEach(interval => {
        totalMs += (interval.end - interval.start);
    });

    // Return months
    return totalMs / (1000 * 60 * 60 * 24 * 30.44); // Average month length
};

const totalYearsExperience = computed(() => {
    if (workExperiences.value.length === 0) return 0;

    // Convert to intervals
    const intervals: { start: number; end: number }[] = workExperiences.value.map(w => {
        const start = w.startDate ? new Date(w.startDate).getTime() : 0;
        const end = w.endDate ? new Date(w.endDate).getTime() : new Date().getTime();
        return { start, end };
    }).filter(i => i.start > 0);

    const totalMonths = calculateMergedDuration(intervals);
    return (totalMonths / 12).toFixed(1) + ' Years';
});

const avgCompanyTenure = computed(() => {
    const companyDurations: Record<string, number> = {};

    workExperiences.value.forEach(w => {
        const company = w.company?.name?.trim();
        if (!company) return;

        const duration = calculateDurationInMonths(w.startDate, w.endDate);
        companyDurations[company] = (companyDurations[company] || 0) + duration;
    });

    const companies = Object.keys(companyDurations);
    if (companies.length === 0) return '0 Months';

    const totalDuration = Object.values(companyDurations).reduce((a, b) => a + b, 0);
    const avgMonths = totalDuration / companies.length;

    if (avgMonths >= 12) {
        return (avgMonths / 12).toFixed(1) + ' Years';
    }
    return Math.round(avgMonths) + ' Months';
});

const categoryDurationDistribution = computed(() => {
    const categoryIntervals: Record<string, { start: number; end: number }[]> = {};

    workExperiences.value.forEach(w => {
        // Find tags of type Category (7)
        const categoryTags = w.tags?.filter(t => t.type === TagType.Category) || [];
        if (categoryTags.length === 0) return;

        const start = w.startDate ? new Date(w.startDate).getTime() : 0;
        const end = w.endDate ? new Date(w.endDate).getTime() : new Date().getTime();
        if (start === 0) return;

        categoryTags.forEach(tag => {
            const name = tag.name || 'Unknown';
            if (!categoryIntervals[name]) {
                categoryIntervals[name] = [];
            }
            categoryIntervals[name].push({ start, end });
        });
    });

    const distribution: Record<string, number> = {};
    Object.keys(categoryIntervals).forEach(category => {
        const intervals = categoryIntervals[category];
        if (intervals) {
            distribution[category] = calculateMergedDuration(intervals);
        }
    });

    // Sort by duration desc
    return Object.fromEntries(
        Object.entries(distribution).sort(([,a], [,b]) => b - a)
    );
});

const totalCategoryMonths = computed(() => {
    // This total is for the progress bar percentage relative to the *sum of all category durations*
    // Note: If I work 1 year as "Full Stack", it counts as 1 year Backend and 1 year Frontend?
    // If so, the sum > total actual time. That's fine for distribution.
    return Object.values(categoryDurationDistribution.value).reduce((a, b) => a + b, 0);
});

const formatDuration = (months: number) => {
    if (months >= 12) {
        return (months / 12).toFixed(1) + ' Years';
    }
    return Math.round(months) + ' Months';
};

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
    const [fetchedTags, fetchedProjects, fetchedWork] = await Promise.all([
      tagService.getAll(),
      projectService.getAll(),
      workExperienceService.getAll()
    ]);
    tags.value = fetchedTags; // Keep all tags for stats, but might want to filter for usage
    // Original filter was: tags.value = fetchedTags.filter(t => t.type !== TagType.Area && t.type !== TagType.Category);
    // Let's keep the filter for the charts if that was the intention, but for "Total Tags" maybe we want all?
    // The previous code filtered them for 'tags.value' which is used for both. Let's stick to previous logic.
    tags.value = fetchedTags.filter(t => t.type !== TagType.Area && t.type !== TagType.Category);
    projects.value = fetchedProjects;
    workExperiences.value = fetchedWork;
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
