<template>
  <TheHeader />
  <div class="auth-page">
    <div class="auth-container">
      <h2>Регистрация</h2>
      <form @submit.prevent="handleRegister" class="auth-form">
        
        <div class="form-group">
          <label>Имя</label>
          <input type="text" v-model="form.firstName" placeholder="Иван" required minlength="2"/>
        </div>

        <div class="form-group">
          <label>Телефон</label>
          <input 
            type="text" 
            :value="form.phone" 
            @input="handlePhoneInput" 
            placeholder="+79991234567" 
            required />
        </div>

        <div class="form-group">
          <label>Пароль</label>
          <input type="password" v-model="form.password" placeholder="Ваш пароль" required minlength="8"/>
        </div>

        <p v-if="errorMessage" class="error-msg">{{ errorMessage }}</p>

        <button type="submit" class="submit-btn" :disabled="isLoading">
          {{ isLoading ? 'Регистрация...' : 'Зарегистрироваться' }}
        </button>
      </form>
      
      <p class="auth-links">
        Уже есть аккаунт? <router-link to="/login">Войти</router-link>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import TheHeader from '@/components/Header.vue';

const form = ref({
  firstName: '',
  phone: '',
  password: ''
});
const errorMessage = ref('');
const isLoading = ref(false);

const authStore = useAuthStore();
const router = useRouter();

const handleRegister = async () => {
  isLoading.value = true;
  errorMessage.value = '';
  
  try {
    await authStore.register(form.value);
    router.push('/');
  } catch (error) {
    errorMessage.value = error.response?.data || 'Ошибка при регистрации. Проверьте данные.';
  } finally {
    isLoading.value = false;
  }
};

const handlePhoneInput = (event) => {
  let inputDigits = event.target.value.replace(/[^\d]/g, '');
  
  if (inputDigits) {
    form.value.phone = '+' + inputDigits;
  } else {
    form.value.phone = '';
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
        font-family: var(--main-font);
        font-weight: 600;
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