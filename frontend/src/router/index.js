import { createRouter, createWebHistory } from 'vue-router';
import Index from '@/views/IndexView.vue';

const routes = [
  { path: '/', component: Index },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    redirect: '/',
    meta: {
      pageTitle: 'Страница не найдена',
    },
  },
];

export const router = createRouter({
  history: createWebHistory(),
  routes,
});
