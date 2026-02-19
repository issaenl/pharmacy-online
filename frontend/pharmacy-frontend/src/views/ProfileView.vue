<template>
  <TheHeader />
  <div class="profile-page">
    <div class="container profile-layout">
      
      <aside class="profile-sidebar">
        <div class="user-brief">
          <div class="user-avatar">{{ userInitials }}</div>
          <h3>{{ authStore.user?.firstName }}</h3>
        </div>

        <nav class="profile-menu">
          <button 
            :class="['menu-item', { active: currentTab === 'info' }]" 
            @click="currentTab = 'info'">
            Личные данные
          </button>
          
          <button 
            :class="['menu-item', { active: currentTab === 'orders' }]" 
            @click="currentTab = 'orders'">
            История заказов
          </button>
        </nav>

        <button @click="handleLogout" class="logout-btn">Выйти из аккаунта</button>
      </aside>

      <main class="profile-content">
        <ProfileInfo v-if="currentTab === 'info'" />
        <ProfileOrders v-if="currentTab === 'orders'" />
      </main>
      
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import TheHeader from '@/components/Header.vue';
import ProfileInfo from '@/components/ProfileInfo.vue';
import ProfileOrders from '@/components/ProfileOrders.vue';

const authStore = useAuthStore();
const router = useRouter();

const currentTab = ref('info'); 

onMounted(() => {
  if (!authStore.token) router.push('/login');
});

const userInitials = computed(() => {
  return authStore.user?.firstName ? authStore.user.firstName.charAt(0).toUpperCase() : 'U';
});

const handleLogout = () => {
  authStore.logout();
  router.push('/');
};
</script>

<style scoped>

    .profile-page { 
        padding: 40px 0; 
        background: #F5F5F5; 
        min-height: 80vh; 
    }

    .container { 
        max-width: 1200px; 
        margin: 0 auto; 
        padding: 0 20px; 
    }

    .profile-layout { 
        display: flex; 
        gap: 30px; 
        align-items: flex-start; 
    }

    .profile-sidebar { 
        flex: 0 0 280px; 
        background: white; 
        border-radius: 20px; 
        padding: 30px 20px; 
        box-shadow: 0 4px 12px rgba(0,0,0,0.05); 
        position: sticky; top: 20px; 
    }

    .user-brief { 
        display: flex; 
        flex-direction: column; 
        align-items: center; 
        margin-bottom: 30px; 
        border-bottom: 1px solid #eee; 
        padding-bottom: 20px; 
    }

    .user-avatar {
        width: 80px; 
        height: 80px; 
        background: #B3CCAE; 
        color: white; 
        border-radius: 50%; 
        display: flex; 
        align-items: center; 
        justify-content: center; 
        font-size: 32px; 
        font-weight: bold; 
        margin-bottom: 15px; 
        font-family: var(--main-font); 
    }

    .user-brief h3 { 
        margin: 0; 
        font-size: 20px; 
        color: #000; 
    }

    .profile-menu { 
        display: flex; 
        flex-direction: column; 
        gap: 10px; 
        margin-bottom: 30px; 
    }

    .menu-item { 
        background: transparent; 
        border: none; 
        text-align: left; 
        padding: 12px 15px; 
        border-radius: 10px; 
        font-size: 16px; 
        cursor: pointer; 
        color: #555; 
        transition: 0.2s; 
        font-family: var(--main-font); 
    }

    .menu-item:hover { 
        background: #F5F5F5; 
    }

    .menu-item.active { 
        background: #689D6D; 
        color: white; 
        font-weight: 600; 
    }

    .logout-btn { 
        width: 100%; 
        background: transparent; 
        border: 1px solid #BB4E58; 
        color: #BB4E58; 
        padding: 12px; 
        border-radius: 10px; 
        font-size: 16px; 
        font-weight: 600; 
        cursor: pointer; 
        transition: 0.2s; 
        font-family: var(--main-font); 
    }

    .logout-btn:hover { 
        background: #BB4E58; 
        color: white; 
    }

    .profile-content { 
        flex: 1; 
    }

    @media (max-width: 768px) {
        .profile-layout { 
            flex-direction: column; 
        }

        .profile-sidebar { 
            width: 100%; 
            flex: none; 
            position: static; 
        }
    }
</style>