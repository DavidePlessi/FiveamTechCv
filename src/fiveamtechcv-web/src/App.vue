<template>
  <v-app>
<!--    &lt;!&ndash; Background Effects &ndash;&gt;-->
<!--    <div id="sky-gradient"></div>-->
<!--    <div class="grid-overlay"></div>-->

    <v-main class="pb-16">
      <router-view />
    </v-main>


    <!-- Person Context Selector -->
    <div class="person-selector-container" v-if="contextStore.people.length > 0" v-show="showPersonSelector">
        <v-autocomplete
            v-model="contextStore.selectedPersonId"
            :items="contextStore.people"
            item-title="name"
            item-value="id"
            label="Filter by Person"
            variant="outlined"
            density="compact"
            hide-details
            clearable
            prepend-inner-icon="mdi-account-filter"
            color="primary"
            base-color="primary"
            class="cyber-input"
            :menu-props="{ location: 'top', zIndex: 10001 }"
        >
            <template #item="{ props, item }">
                <v-list-item v-bind="props" :title="`${item.raw.name} ${item.raw.lastName}`" :subtitle="item.raw.summary?.find(s => s.language === 'EN')?.value"></v-list-item>
            </template>
            <template #selection="{ item }">
               {{ item.raw.name }} {{ item.raw.lastName }}
            </template>
        </v-autocomplete>
    </div>

    <!-- Cyber Dock Navigation -->
    <nav class="cyber-dock">
      <!-- Person Filter Toggle -->
      <!-- <a class="dock-item" :class="{ active: showPersonSelector || contextStore.selectedPersonId }" @click="togglePersonSelector" style="cursor: pointer;">
        <v-icon :color="contextStore.selectedPersonId ? 'primary' : ''">mdi-account-filter</v-icon>
        <span class="dock-tooltip">{{ personSelectorTooltip }}</span>
      </a> -->

      <router-link to="/" class="dock-item" :class="{ active: $route.path === '/' }">
        <v-icon>mdi-home</v-icon>
        <span class="dock-tooltip">Dashboard</span>
      </router-link>
      <router-link to="/companies" class="dock-item" :class="{ active: $route.path === '/companies' }">
        <v-icon>mdi-domain</v-icon>
        <span class="dock-tooltip">Companies</span>
      </router-link>
      <router-link to="/people" class="dock-item" :class="{ active: $route.path === '/people' }">
        <v-icon>mdi-account</v-icon>
        <span class="dock-tooltip">People</span>
      </router-link>
      <router-link to="/work-experiences" class="dock-item" :class="{ active: $route.path === '/work-experiences' }">
        <v-icon>mdi-briefcase</v-icon>
        <span class="dock-tooltip">Work Experiences</span>
      </router-link>
      <router-link to="/projects" class="dock-item" :class="{ active: $route.path === '/projects' }">
        <v-icon>mdi-rocket-launch</v-icon>
        <span class="dock-tooltip">Projects</span>
      </router-link>
      <router-link to="/tags" class="dock-item" :class="{ active: $route.path === '/tags' }">
        <v-icon>mdi-tag-multiple</v-icon>
        <span class="dock-tooltip">Tags</span>
      </router-link>
      <router-link to="/graphql" class="dock-item" :class="{ active: $route.path === '/graphql' }">
        <v-icon>mdi-graphql</v-icon>
        <span class="dock-tooltip">GraphQL</span>
      </router-link>

      <a v-if="authStore.isAuthenticated" @click="authStore.logout()" class="dock-item" style="cursor: pointer;">
         <v-icon>mdi-logout</v-icon>
         <span class="dock-tooltip">Logout</span>
      </a>
      <router-link v-else to="/login" class="dock-item" :class="{ active: $route.path === '/login' }">
         <v-icon>mdi-login</v-icon>
         <span class="dock-tooltip">Login</span>
      </router-link>
    </nav>
  </v-app>
</template>

<script lang="ts" setup>
import { useAuthStore } from '@/stores/auth';
import { useContextStore } from '@/stores/context';
import { onMounted, ref, computed } from 'vue';

const authStore = useAuthStore();
const contextStore = useContextStore();
const showPersonSelector = ref(false);

const togglePersonSelector = () => {
    showPersonSelector.value = !showPersonSelector.value;
};

const personSelectorTooltip = computed(() => {
    if (contextStore.selectedPersonId) {
        const person = contextStore.people.find(p => p.id === contextStore.selectedPersonId);
        return person ? `Filtered by: ${person.name} ${person.lastName}` : 'Filter by Person';
    }
    return 'Filter by Person';
});

onMounted(async () => {
    await contextStore.fetchPeople();
});
</script>

<style scoped>
.person-selector-container {
    position: fixed;
    bottom: 90px;
    left: 50%;
    transform: translateX(-50%);
    width: 300px;
    z-index: 10000; /* Ensure higher than everything including nav (9998) */
    background: rgba(10, 10, 10, 0.95); /* More opaque */
    backdrop-filter: blur(12px);
    border: 1px solid rgba(0, 243, 255, 0.5);
    border-radius: 8px;
    padding: 12px;
    box-shadow: 0 0 20px rgba(0, 243, 255, 0.3);
}

/* Mobile responsive adjustments */
@media (max-width: 600px) {
    .person-selector-container {
        width: 90%;
        bottom: 80px;
    }
}
</style>
