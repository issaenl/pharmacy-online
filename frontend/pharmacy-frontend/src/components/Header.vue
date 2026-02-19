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

        <router-link to="/" class="logo">УниМед</router-link>
        
        <router-link to="/full-catalog" class="catalog-btn desktop-only">
            <img src="/assets/FilterLeft.svg" alt="Каталог">
            <span class="catalog-text">Каталог</span>
        </router-link>

        <SearchBar />

        <div class="user-menu">
            <div class="menu-item">
                <img src="/assets/Heart.svg" alt="Избранное" class="icon-img">
                <span class="desktop-only">Избранное</span>
            </div>
            <div class="menu-item">
                <img src="/assets/Basket.svg" alt="Корзина" class="icon-img">
                <span class="desktop-only">Корзина</span>
            </div>
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
import { ref, onMounted, computed } from 'vue';
import SearchBar from '@/components/SearchBar.vue';
import api from '@/api/api';
import { useAuthStore } from '@/stores/authStore';

const authStore = useAuthStore();
const isMenuOpen = ref(false);
const allCategories = ref([]);

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value;
};

const targetCategories = [
  { label: 'Лекарства', dbNames: ['Жаропонижающие', 'Антибиотики'] }, 
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
        font-size: 32px;
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

    .bar-top { 
        transform: rotate(45deg) translate(5px, 6px); 
    }

    .bar-mid { 
        opacity: 0; 
    }

    .bar-bot { 
        transform: rotate(-45deg) translate(5px, -6px); 
    }

    .catalog-btn {
        background: #B3CCAE;
        color: white;
        text-decoration: none;
        border: none;
        padding: 10px 20px;
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
        font-size: 16px;
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
        font-size: 16px;
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

    .slide-enter-active, .slide-leave-active { 
        transition: 0.3s; 
    }

    .slide-enter-from, .slide-leave-to { 
        transform: translateY(-10px); 
        opacity: 0; 
    }

    @media (max-width: 992px) {
        .desktop-only { 
            display: none !important; 
        }

        .burger-btn { 
            display: flex; 
        }

        .search-bar {
            /* order: 3; */
            width: 100%;
            margin-top: 10px;
        }
    }

    @media (max-width: 600px) {
        .logo { 
            font-size: 24px; 
        }

        .user-menu { 
            gap: 10px; 
        }

        .search-bar input, .search-btn { 
            font-size: 16px; 
            padding: 8px; 
        }

         .search-bar {
            order: 3;
        }
    }
</style>