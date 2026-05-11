<template>
  <header class="header">
    <div class="header__main">
      <div class="container main-content">
        <button class="burger-btn" @click="toggleMenu">
        <span :class="{ 'bar-top': isMenuOpen }"></span>
        <span :class="{ 'bar-mid': isMenuOpen }"></span>
        <span :class="{ 'bar-bot': isMenuOpen }"></span>
        </button>

        <transition name="slide">
            <nav v-if="isMenuOpen" class="mobile-menu">
                <router-link to='/full-catalog' @click="isMenuOpen = false">Все товары</router-link>
                <router-link 
                v-for="cat in navCategories" 
                :key="cat.id"
                :to="{ path: '/catalog', query: { categoryId: cat.id, categoryName: cat.name }}"
                @click="isMenuOpen = false">
                {{ cat.name }}
                </router-link>
            </nav>
        </transition>

        <router-link to="/" class="logo">Уни<span class="logo-span">Мед</span></router-link>
        
        <router-link to="/full-catalog" class="catalog-btn desktop-only">
            <img src="/assets/FilterLeft.svg" alt="Каталог">
            <span class="catalog-text">Каталог</span>
        </router-link>

        <SearchBar />

        <div class="user-menu">
            <div class="menu-item notif-wrapper" @click.stop="toggleNotifications" v-if="authStore.user">
                <div class="icon-container">
                    <svg class="icon-img" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
                        <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
                    </svg>
                    <span v-if="notificationStore.unreadCount > 0" class="cart-badge">{{ notificationStore.unreadCount }}</span>
                </div>
                <span class="desktop-only">Уведомления</span>

                <transition name="fade">
                    <div v-if="isNotifOpen" class="notif-dropdown" @click.stop>
                        
                        <div v-if="notificationStore.notifications.length === 0" class="notif-empty">
                            Нет новых уведомлений
                        </div>
                        
                        <div class="notif-list" v-else>
                            <div 
                              v-for="n in notificationStore.notifications" 
                              :key="n.id" 
                              class="notif-item" 
                              :class="{ 'is-read': n.isRead }"
                            >
                                <div class="notif-top">
                                    <div class="notif-title-group">
                                        <span class="notif-dot" v-if="!n.isRead"></span>
                                        <span class="notif-title">Уведомление</span>
                                    </div>
                                    <div class="notif-actions">
                                        <span class="notif-time">{{ timeAgo(n.createdAt) }}</span>
                                        <button class="notif-close-btn" @click.stop="removeNotif(n.id)" title="Удалить">✕</button>
                                    </div>
                                </div>
                                <div class="notif-message">{{ n.message }}</div>
                            </div>
                        </div>
                    </div>
                </transition>
            </div>

            <router-link to="/favorites" class="menu-item" style="text-decoration: none;">
                <div class="icon-container">
                <svg class="icon-img"
                    width="24" 
                    height="24" 
                    viewBox="0 0 24 24" 
                    fill="none" 
                    stroke="currentColor" 
                    stroke-width="2" 
                    stroke-linecap="round" 
                    stroke-linejoin="round">
                    <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
                </svg>
                <span v-if="favoriteStore.items.length > 0" class="cart-badge">{{ favoriteStore.items.length }}</span>
                </div>
                <span class="desktop-only">Избранное</span>
            </router-link>

            <router-link to="/cart" class="menu-item" style="text-decoration: none;">
                <div class="icon-container">
                    <img src="/assets/Basket.svg" alt="Корзина" class="icon-img">
                    <span v-if="cartStore.totalItems > 0" class="cart-badge">{{ cartStore.totalItems }}</span>
                </div>
                <span class="desktop-only">Корзина</span>
            </router-link>
            
            <router-link :to="authStore.user ? '/profile' : '/login'" class="menu-item" style="text-decoration: none;">
                <img src="/assets/User.svg" alt="Профиль" class="icon-img">
                <span class="desktop-only">{{ authStore.user ? authStore.user.firstName : 'Войти' }}</span>
            </router-link>
        </div>
      </div>
    </div>

    <nav class="header__nav desktop-only">
      <div class="container nav-list">
        <router-link 
          v-for="cat in navCategories" 
          :key="cat.id"
          :to="{ path: '/catalog', query: { categoryId: cat.id, categoryName: cat.name }}">
          {{ cat.name }}
        </router-link>
      </div>
    </nav>
  </header>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue';
import SearchBar from '@/components/SearchBar.vue';
import api from '@/api/api';
import { useAuthStore } from '@/stores/authStore';
import { useCartStore } from '@/stores/cartStore';
import { useFavoriteStore } from '@/stores/favoriteStore';
import { useNotificationStore } from '@/stores/notificationStore';

const authStore = useAuthStore();
const cartStore = useCartStore();
const favoriteStore = useFavoriteStore();
const notificationStore = useNotificationStore();

const isMenuOpen = ref(false);
const allCategories = ref([]);

const isNotifOpen = ref(false);

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value;
};

const toggleNotifications = async () => {
  isNotifOpen.value = !isNotifOpen.value;
  if (isNotifOpen.value && notificationStore.unreadCount > 0) {
    await notificationStore.markAsRead();
  }
};

const closeNotifOnClickOutside = () => {
  if (isNotifOpen.value) {
    isNotifOpen.value = false;
  }
};

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString('ru-RU', { 
    day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' 
  });
};

const targetCategories = [
  { label: 'Антибиотики', dbNames: ['Жаропонижающие', 'Антибиотики'] }, 
  { label: 'Витамины и БАДы', dbNames: ['Витамины', 'Минеральные добавки'] },
  { label: 'Нервная система', dbNames: ['Антидепрессанты', 'Снотворные и успокоительные средства'] },
  { label: 'Гигиена', dbNames: ['Антисептики'] },
  { label: 'Косметика', dbNames: ['Препараты для лечения акне'] }
];

onMounted(async () => {
  try {
    const response = await api.get('/Categories');
    allCategories.value = response.data;
  } catch (error) {
    console.error("Ошибка загрузки:", error);
  }

  await cartStore.loadCart();
  await favoriteStore.loadFavorites();

  if (authStore.user) {
    await notificationStore.fetchNotifications();
    await notificationStore.initSignalR(); 
  }

  document.addEventListener('click', closeNotifOnClickOutside);
});
const timeAgo = (dateString) => {
  if (!dateString) return '';
  const past = new Date(dateString.endsWith('Z') ? dateString : dateString + 'Z');
  const now = new Date();
  const diffMs = now - past;
  const diffMins = Math.floor(diffMs / 60000);
  
  if (diffMins < 1) return 'только что';
  if (diffMins < 60) return `${diffMins}м назад`;
  const diffHours = Math.floor(diffMins / 60);
  if (diffHours < 24) return `${diffHours}ч назад`;
  const diffDays = Math.floor(diffHours / 24);
  return `${diffDays}д назад`;
};

const removeNotif = async (id) => {
  await notificationStore.removeNotification(id);
};

onUnmounted(async () => {
  document.removeEventListener('click', closeNotifOnClickOutside);
  await notificationStore.stopSignalR();
});

const navCategories = computed(() => {
  return targetCategories.map(target => {
    const foundIds = allCategories.value
      .filter(c => target.dbNames.includes(c.name))
      .map(c => c.id);

    return {
      id: foundIds.join(','), 
      name: target.label 
    };
  }).filter(c => c.id !== "");
});
</script>

<style scoped>
    .header__main {
        width: 100%;
        padding: 20px 0;
        background: #689D6D;
        border-bottom: 1px solid #fff;
        position: relative;
        z-index: 100;
    }

    .logo-span
    {
        color: #BB4E58;
    }

    .container {
        max-width: 1200px;
        margin: 0 auto;
        padding: 0 20px;
        display: flex;
        align-items: center;
    }

    .main-content {
        justify-content: space-between;
        gap: 15px;
        flex-wrap: wrap;
    }

    .logo {
        font-weight: bold;
        color: #fff;
        font-family: var(--logo-font);
        font-size: 38px;
        text-decoration: none;
    }

    .burger-btn {
        display: none;
        flex-direction: column;
        justify-content: space-around;
        width: 30px;
        height: 24px;
        background: transparent;
        border: none;
        cursor: pointer;
        padding: 0;
        z-index: 1001;
    }

    .burger-btn span {
        width: 30px;
        height: 3px;
        background: white;
        border-radius: 10px;
        transition: 0.3s;
    }

    .bar-top { transform: rotate(45deg) translate(5px, 6px); }
    .bar-mid { opacity: 0; }
    .bar-bot { transform: rotate(-45deg) translate(5px, -6px); }

    .catalog-btn {
        background: #B3CCAE;
        color: white;
        text-decoration: none;
        border: none;
        padding: 8px 20px;
        border-radius: 10px;
        display: flex;
        align-items: center;
        gap: 8px;
        cursor: pointer;
        font-family: var(--main-font);
        font-size: 20px;
        font-weight: 500;
    }

    .user-menu {
        display: flex;
        gap: 20px;
    }

    .menu-item {
        display: flex;
        flex-direction: column;
        align-items: center;
        color: #fff;
        cursor: pointer;
        font-size: 18px;
    }

    .notif-wrapper {
        position: relative;
    }

    .notif-dropdown {
        position: absolute;
        top: 55px; 
        right: -80px; 
        width: 360px;
        background: white;
        box-shadow: 0 10px 30px rgba(0,0,0,0.15);
        border-radius: 12px;
        padding: 0;
        z-index: 1000;
        color: #333;
        cursor: default;
        text-align: left;
        overflow: hidden;
    }

    .notif-dropdown::before {
        content: '';
        position: absolute;
        top: -8px;
        right: 88px;
        border-left: 8px solid transparent;
        border-right: 8px solid transparent;
        border-bottom: 8px solid white;
    }

    .notif-empty {
        padding: 30px;
        text-align: center;
        color: #888;
        font-size: 20px;
    }

    .notif-list {
        max-height: 400px;
        overflow-y: auto;
    }

    .notif-item {
        padding: 16px 20px;
        border-bottom: 1px solid #f0f0f0;
        display: flex;
        flex-direction: column;
        gap: 8px;
        transition: background 0.2s;
    }

    .notif-item:hover { background: #fdfdfd; }
    .notif-item:last-child { border-bottom: none; }

    .notif-top {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .notif-title-group {
        display: flex;
        align-items: center;
        gap: 8px;
    }

    .notif-dot {
        width: 8px;
        height: 8px;
        background-color: #689D6D;
        border-radius: 50%;
    }

    .notif-title {
        font-weight: 700;
        font-size: 18px;
        color: #2c3e50;
    }

    .notif-item.is-read .notif-title {
        color: #555;
        font-weight: 600;
    }

    .notif-actions {
        display: flex;
        align-items: center;
        gap: 12px;
    }

    .notif-time {
        color: #999;
        font-size: 16px;
    }

    .notif-close-btn {
        background: none;
        border: none;
        color: #ccc;
        cursor: pointer;
        font-size: 14px;
        padding: 2px 6px;
        border-radius: 6px;
        transition: 0.2s;
        line-height: 1;
    }

    .notif-close-btn:hover {
        background: #f0f0f0;
        color: #BB4E58;
    }

    .notif-message {
        font-size: 16px;
        line-height: 1.5;
        color: #666;
        margin: 0;
    }

    .fade-enter-active, .fade-leave-active { transition: opacity 0.2s, transform 0.2s; }
    .fade-enter-from, .fade-leave-to { opacity: 0; transform: translateY(-10px); }

    .icon-container {
        position: relative;
        display: flex;
    }

    .cart-badge {
        position: absolute;
        top: -6px;
        right: -10px;
        background-color: #BB4E58; 
        color: white;
        font-size: 14px;
        font-weight: 600;
        padding: 2px 6px;
        border-radius: 12px;
        line-height: 1;
        border: 2px solid #689D6D; 
    }

    .icon-img {
        width: 24px;
        height: 24px;
        margin-bottom: 4px;
    }

    .header__nav {
        background: #689D6D;
        color: white;
        padding: 20px 0;
    }

    .nav-list {
        display: flex;
        gap: 30px;
        justify-content: center;
    }

    .nav-list a {
        font-family: var(--main-font);
        font-weight: 600;
        font-size: 18px;
        color: white;
        text-decoration: none;
    }

    .mobile-menu {
        position: absolute;
        top: 100%;
        left: 0;
        width: 100%;
        background: #689D6D;
        padding: 20px;
        display: flex;
        flex-direction: column;
        gap: 15px;
        border-top: 1px solid rgba(255,255,255,0.3);
        box-shadow: 0 10px 20px rgba(0,0,0,0.1);
        z-index: 99;
    }

    .mobile-menu a {
        color: white;
        text-decoration: none;
        font-size: 18px;
        font-weight: 500;
    }

    .slide-enter-active, .slide-leave-active { transition: 0.3s; }
    .slide-enter-from, .slide-leave-to { transform: translateY(-10px); opacity: 0; }

    @media (max-width: 992px) {
        .desktop-only { display: none !important; }
        .burger-btn { display: flex; }
        .search-bar { width: 100%; margin-top: 10px; }
    }

    @media (max-width: 600px) {
        .logo { font-size: 24px; }
        .user-menu { gap: 10px; }
        .search-bar input, .search-btn { font-size: 16px; padding: 8px; }
        .search-bar { order: 3; }
        
        .notif-dropdown {
            position: fixed;
            top: 70px;
            right: 10px;
            left: 10px;
            width: auto;
            max-height: 70vh;
        }
        .notif-dropdown::before { display: none; }
    }
</style>