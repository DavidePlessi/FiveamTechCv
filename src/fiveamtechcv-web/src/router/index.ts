/**
 * router/index.ts
 */

import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

// Views
import LoginPage from '@/views/LoginPage.vue';
import MainLayout from '@/layouts/MainLayout.vue';
import ProjectView from '@/views/ProjectView.vue';
import TagView from '@/views/TagView.vue';
import DashboardView from '@/views/DashboardView.vue';

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: LoginPage,
  },
  {
    path: '/',
    component: MainLayout,
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'Dashboard',
        component: DashboardView,
      },
      {
        path: 'projects',
        name: 'Projects',
        component: ProjectView,
      },
      {
        path: 'tags',
        name: 'Tags',
        component: TagView,
      },
      // {
      //   path: 'work-experiences',
      //   name: 'WorkExperiences',
      //   component: () => import('@/views/WorkExperienceView.vue'),
      // },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login');
  } else if (to.path === '/login' && authStore.isAuthenticated) {
    next('/');
  } else {
    next();
  }
});

// Workaround for dynamic import errors (keeping existing logic just in case)
router.onError((err, to) => {
  if (err?.message?.includes?.('Failed to fetch dynamically imported module')) {
    if (!localStorage.getItem('vuetify:dynamic-reload')) {
      console.log('Reloading page to fix dynamic import error');
      localStorage.setItem('vuetify:dynamic-reload', 'true');
      location.assign(to.fullPath);
    } else {
      console.error('Dynamic import error, reloading page did not fix it', err);
    }
  } else {
    console.error(err);
  }
});

router.isReady().then(() => {
  localStorage.removeItem('vuetify:dynamic-reload');
});

export default router;
