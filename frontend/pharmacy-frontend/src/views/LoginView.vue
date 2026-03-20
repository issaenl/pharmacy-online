<template>
  <TheHeader />
  <div class="auth-page">
    <div class="auth-container">
      <h2>Вход</h2>
      <form @submit.prevent="handleLogin" class="auth-form">
        
        <div class="form-group">
          <label>Телефон или Email</label>
          <input 
            type="text" 
            :value="loginCredential" 
            @input="handleLoginInput" 
            placeholder="+375291234567 или email@example.com" 
            required />
        </div>

        <div class="form-group">
          <label>Пароль</label>
          <input type="password" v-model="password" placeholder="Ваш пароль" required />
        </div>

        <p v-if="errorMessage" class="error-msg">{{ errorMessage }}</p>

        <button type="submit" class="submit-btn" :disabled="isLoading">
          {{ isLoading ? 'Вход...' : 'Войти' }}
        </button>
      </form>
      
      <p class="auth-links">
        Нет аккаунта? <router-link to="/register">Зарегистрироваться</router-link>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import TheHeader from '@/components/Header.vue';

const loginCredential = ref('');
const password = ref('');
const errorMessage = ref('');
const isLoading = ref(false);

const authStore = useAuthStore();
const router = useRouter();
const route = useRoute();

const handleLoginInput = (event) => {
  let val = event.target.value;
  
  if (/^[\+\d]/.test(val) && !val.includes('@') && !/[a-zA-Z]/.test(val)) {
    let inputDigits = val.replace(/[^\d]/g, '');
    loginCredential.value = inputDigits ? '+' + inputDigits : '';
  } else {
    loginCredential.value = val;
  }
};

const handleLogin = async () => {
  isLoading.value = true;
  errorMessage.value = '';
  
  try {
    await authStore.login(loginCredential.value, password.value);
    const redirectPath = route.query.redirect;

    if (redirectPath) {
      router.push(redirectPath); 
    } else {
      const userRole = authStore.user?.role;
      if (userRole === 1 || userRole === 'Admin' || userRole === 2 || userRole === 'PharmacyAdmin') {
         router.push('/admin');
      } else {
         router.push('/'); 
      }
    }
  } catch (error) {
    errorMessage.value = error;
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
    .auth-page {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 70vh;
        background-color: #F5F5F5;
    }

    .auth-container {
        background: white;
        padding: 40px;
        border-radius: 20px;
        box-shadow: 0 10px 25px rgba(0,0,0,0.05);
        width: 100%;
        max-width: 400px;
        text-align: center;
    }

    .auth-container h2 {
        margin-bottom: 20px;
        font-family: var(--main-font);
        color: #000;
    }

    .form-group {
        margin-bottom: 15px;
        text-align: left;
    }

    .form-group label {
        display: block;
        margin-bottom: 5px;
        font-size: 18px;
        color: #000;
    }

    .form-group input {
        width: 100%;
        padding: 12px;
        border: none;
        border-radius: 10px;
        background-color: #F5F5F5;
        font-family: var(--main-font);
        font-size: 16px;
    }

    .submit-btn {
        width: 100%;
        padding: 12px;
        background: #689D6D;
        color: white;
        border: none;
        border-radius: 10px;
        font-size: 18px;
        font-weight: 600;
        font-family: var(--main-font);
        cursor: pointer;
        margin-top: 10px;
        transition: 0.2s;
    }

    .submit-btn:hover { 
        background: #5a885e; 
    }

    .submit-btn:disabled { 
        background: #B3CCAE; 
        cursor: not-allowed; 
    }

    .error-msg { 
        color: #BB4E58; 
        font-weight: 600;
        font-size: 16px; 
        margin-bottom: 10px; 
    }

    .auth-links { 
        margin-top: 20px; 
        font-size: 16px; 
    }

    .auth-links a { 
        color: #689D6D; 
        text-decoration: none; 
        font-weight: 600; 
    }
</style>