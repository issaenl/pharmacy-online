import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import HomeView from '../views/HomeView.vue'; 
import ProductDetailView from '../views/ProductDetailView.vue';
import ProductListView from '../views/ProductListView.vue'
import CatalogView from '../views/CatalogView.vue'
import FilterCatalogView from '../views/FilterCatalogView.vue';
import LoginView from '@/views/LoginView.vue';
import RegisterView from '@/views/RegisterView.vue';
import ProfileView from '@/views/ProfileView.vue';
import CartView from '@/views/CartView.vue';
import FavoriteView from '@/views/FavoriteView.vue';
import AdminMain from '@/views/admin/AdminMain.vue';
import AdminProducts from '@/views/admin/AdminProducts.vue';
import AdminPharmacies from '@/views/admin/AdminPharmacies.vue';
import AdminStock from '@/views/admin/AdminStock.vue';

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
  },
  {
    path: '/catalog',
    name: 'catalog',
    component: CatalogView
  },
  {
    path: '/full-catalog',
    name: 'full-catalog',
    component: FilterCatalogView
  },
  {
    path: '/login',
    name: 'login',
    component: LoginView
  },
  {
    path: '/register',
    name: 'register',
    component: RegisterView
  },
  {
    path: '/profile',
    name: 'profile',
    component: ProfileView
  },
  {
    path: '/cart',
    name: 'cart',
    component: CartView
  },
  {
    path: '/favorites',
    name: 'favorites',
    component: FavoriteView
  },


  {
    path: '/admin',
    component: AdminMain,
    meta: { requiresAuth: true, requiresAdmin: true }, 
    children: [
      {
        path: 'products',
        name: 'admin-products',
        component: AdminProducts
      },
      {
        path: 'pharmacies',
        name: 'admin-pharmacies',
        component: AdminPharmacies
      },
      {
        path: 'stocks',
        name: 'admin-stocks',
        component: AdminStock
      }
    ]
  }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  const isAuthenticated = !!authStore.token;

  const userRole = authStore.user?.role; 
  const isSuperAdmin = userRole === 2 || userRole === 'Admin';

  if (to.meta.requiresAdmin) {
    if (!isAuthenticated) {
      next({ name: 'login', query: { redirect: to.fullPath } });
    } else if (!isSuperAdmin) {
      next({ name: 'home' });
    } else {
      next();
    }
  } else if (to.meta.requiresAuth && !isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath } });
  } else {
    next();
  }
});

export default router;