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
import AdminCategory from '@/views/admin/AdminCategory.vue';
import AdminOrder from '@/views/admin/AdminOrder.vue';
import AdminUsers from '@/views/admin/AdminUsers.vue';
import AdminStats from '@/views/admin/AdminStats.vue';
import AdminCustomers from '@/views/admin/AdminCustomers.vue';
import PharmacyView from '@/views/PharmacyView.vue';
import AdminReviews from '@/views/admin/AdminReviews.vue';

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
    path: '/pharmacy/:id',
    name: 'pahrmacy-detail',
    component: PharmacyView
  },

  {
    path: '/admin',
    component: AdminMain,
    meta: { requiresAuth: true, requiresAdmin: true }, 
    children: [
      {
        path: '',
        name: 'admin-stats',
        component: AdminStats
      },
      {
        path: 'products',
        name: 'admin-products',
        component: AdminProducts,
        meta: { superAdminOnly: true }
      },
      {
        path: 'pharmacies',
        name: 'admin-pharmacies',
        component: AdminPharmacies,
        meta: { superAdminOnly: true }
      },
      {
        path: 'categories',
        name: 'admin-categories',
        component: AdminCategory,
        meta: { superAdminOnly: true }
      },
      {
        path: 'users',
        name: 'admin-users',
        component: AdminUsers,
        meta: { superAdminOnly: true }
      },
       {
        path: 'customers',
        name: 'admin-customers',
        component: AdminCustomers,
        meta: { superAdminOnly: true }
      },
      {
        path: 'reviews',
        name: 'admin-reviews',
        component: AdminReviews,
        meta: { superAdminOnly: true }
      },
      {
        path: 'stocks',
        name: 'admin-stocks',
        component: AdminStock
      },
      {
        path: 'orders',
        name: 'admin-orders',
        component: AdminOrder
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
  const isPharmacyAdmin = userRole === 1 || userRole === 'PharmacyAdmin';
  const hasAdminAccess = isSuperAdmin || isPharmacyAdmin;

  if (to.meta.requiresAdmin) {
    if (!isAuthenticated) {
      next({ name: 'login', query: { redirect: to.fullPath } });
    } else if (!hasAdminAccess) {
      next({ name: 'home' });
    } else if (to.meta.superAdminOnly && !isSuperAdmin) {
      next({ name: 'admin-orders' });
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