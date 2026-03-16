<template>
  <div class="admin-layout">
    <aside class="sidebar">
      <div class="logo">
        <h2>Админ-панель</h2>
      </div>
      <nav class="menu">
        <router-link to="/admin" exact-active-class="active">Главная</router-link>
        <router-link to="/admin/products" active-class="active">Продукты</router-link>
        <router-link to="/admin/categories" active-class="active">Категории</router-link>
        <router-link to="/admin/pharmacies" active-class="active">Аптеки</router-link>
        <router-link to="/admin/stocks" active-class="active">Наличие</router-link>
        </nav>
      <button @click="handleLogout" class="logout-btn">Выйти</button>
    </aside>

    <main class="content">
      <router-view /> 
    </main>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/stores/authStore';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const handleLogout = () => {
  authStore.logout();
  router.push('/login');
};
</script>

<style scoped>
.admin-layout {
  display: flex;
  min-height: 100vh;
  background-color: var(--background-color, #F5F5F5);
}

.sidebar {
  width: 250px;
  min-width: 250px; 
  max-width: 250px; 
  height: 100vh;  
  position: sticky; 
  top: 0;
  background-color: #FFFFFF;
  box-shadow: 2px 0 5px rgba(0,0,0,0.05);
  display: flex;
  flex-direction: column;
  padding: 20px 0;
}

.logo {
  padding: 0 20px 20px;
  border-bottom: 1px solid #eee;
  margin-bottom: 20px;
  color: var(--primary-color, #689D6D);
}

.menu {
  display: flex;
  flex-direction: column;
  flex-grow: 1;
}

.menu a {
  padding: 15px 20px;
  text-decoration: none;
  color: #333;
  font-weight: 500;
  transition: 0.2s;
}

.menu a:hover {
  background-color: var(--background-color, #F5F5F5);
}

.menu a.active {
  background-color: var(--primary-light, #B3CCAE);
  color: var(--primary-color, #689D6D);
  border-right: 4px solid var(--primary-color, #689D6D);
}

.logout-btn {
  margin: 20px;
  padding: 10px;
  background-color: var(--accent-color, #BB4E58);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: bold;
  font-family: var(--main-font);
  font-size: 20px;
}

.content {
  flex-grow: 1;
  padding: 30px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}
</style>