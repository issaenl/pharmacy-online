import { createRouter, createWebHistory } from 'vue-router';
import HomeView from '../views/HomeView.vue'; 
import ProductDetailView from '../views/ProductDetailView.vue';
import ProductListView from '../views/ProductListView.vue'

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomeView
  },
  {
    path: '/new',
    name: 'new-products',
    component: ProductListView
  },
  {
    path: '/popular',
    name: 'popular-products',
    component: ProductListView
  },
  {
    path: '/product/:id',
    name: 'product',
    component: ProductDetailView
  }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

export default router;