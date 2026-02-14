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
            <router-link to="/catalog" @click="isMenuOpen = false">Каталог</router-link>
            <a href="#">Лекарства</a>
            <a href="#">Витамины и БАДы</a>
            <a href="#">Медицинские изделия</a>
            <a href="#">Гигиена</a>
            <a href="#">Косметика</a>
        </nav>
        </transition>

        <router-link to="/" class="logo">УниМед</router-link>
        
        <button class="catalog-btn desktop-only">
            <img src="/assets/FilterLeft.svg" alt="Каталог">
            <span class="catalog-text">Каталог</span>
        </button>

        <div class="search-bar">
          <input type="text" placeholder="Поиск лекарств" />
          <button class="search-btn">Найти</button>
        </div>

        <div class="user-menu">
            <div class="menu-item">
                <img src="/assets/Heart.svg" alt="Избранное" class="icon-img">
                <span class="desktop-only">Избранное</span>
            </div>
            <div class="menu-item">
                <img src="/assets/Basket.svg" alt="Корзина" class="icon-img">
                <span class="desktop-only">Корзина</span>
            </div>
            <div class="menu-item">
                <img src="/assets/User.svg" alt="Войти" class="icon-img">
                <span class="desktop-only">Войти</span>
            </div>
        </div>
      </div>
    </div>

    <nav class="header__nav desktop-only">
      <div class="container nav-list">
        <a href="#">Лекарства</a>
        <a href="#">Витамины и БАДы</a>
        <a href="#">Медицинские изделия</a>
        <a href="#">Гигиена</a>
        <a href="#">Косметика</a>
      </div>
    </nav>
  </header>
</template>

<script setup>
import { ref } from 'vue';

const isMenuOpen = ref(false);

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value;
};
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
        border: none;
        padding: 10px 20px;
        border-radius: 10px;
        display: flex;
        align-items: center;
        gap: 8px;
        cursor: pointer;
        font-family: var(--main-font);
        font-size: 20px;
    }

    .search-bar {
        display: flex;
        flex: 1;
        min-width: 280px;
    }

    .search-bar input {
        flex: 1;
        padding: 10px;
        border: none;
        border-radius: 10px 0 0 10px;
        font-family: var(--main-font);
        font-size: 20px;
    }

    .search-btn {
        background: #B3CCAE;
        color: white;
        border: none;
        padding: 0 20px;
        border-radius: 0 10px 10px 0;
        font-family: var(--main-font);
        font-size: 20px;
        cursor: pointer;
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