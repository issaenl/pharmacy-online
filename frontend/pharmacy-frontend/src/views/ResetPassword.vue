<template>
  <TheHeader />
  <div class="auth-page">
    <div class="auth-container">
      <h2>Новый пароль</h2>
      
      <div v-if="isSuccess" class="success-msg">
        {{ successMessage }} <br/>
        <router-link to="/login">Перейти ко входу</router-link>
      </div>

      <form v-else @submit.prevent="handleReset" class="auth-form">
        <div class="form-group">
          <label>Новый пароль</label>
          <input 
            type="password" 
            v-model="newPassword" 
            placeholder="Минимум 8 символов" 
            required 
            minlength="8"
          />
        </div>

        <p v-if="errorMessage" class="error-msg">{{ errorMessage }}</p>

        <button type="submit" class="submit-btn" :disabled="isLoading">
          {{ isLoading ? 'Сохранение...' : 'Сохранить пароль' }}
        </button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import TheHeader from '@/components/Header.vue';

const newPassword = ref('');
const errorMessage = ref('');
const successMessage = ref('');
const isLoading = ref(false);
const isSuccess = ref(false);

const authStore = useAuthStore();
const route = useRoute();

const token = ref('');
const email = ref('');

onMounted(() => {
  token.value = route.query.token;
  email.value = route.query.email;
  
  if (!token.value || !email.value) {
    errorMessage.value = "Неверная ссылка для восстановления.";
  }
});

const handleReset = async () => {
  if (!token.value || !email.value) return;

  isLoading.value = true;
  errorMessage.value = '';
  
  try {
    const msg = await authStore.resetPassword({
      email: email.value,
      token: token.value,
      newPassword: newPassword.value
    });
    successMessage.value = msg;
    isSuccess.value = true;
  } catch (error) {
    errorMessage.value = error;
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
.auth-page { display: flex; justify-content: center; align-items: center; min-height: 70vh; background-color: #F5F5F5; }
.auth-container { background: white; padding: 40px; border-radius: 20px; box-shadow: 0 10px 25px rgba(0,0,0,0.05); width: 100%; max-width: 400px; text-align: center; }
.auth-container h2 { margin-bottom: 20px; font-family: var(--main-font); color: #000; }
.form-group { margin-bottom: 15px; text-align: left; }
.form-group label { display: block; margin-bottom: 5px; font-size: 18px; color: #000; }
.form-group input { width: 100%; padding: 12px; border: none; border-radius: 10px; background-color: #F5F5F5; font-family: var(--main-font); font-size: 16px; }
.submit-btn { width: 100%; padding: 12px; background: #689D6D; color: white; border: none; border-radius: 10px; font-size: 18px; font-weight: 600; font-family: var(--main-font); cursor: pointer; margin-top: 10px; transition: 0.2s; }
.submit-btn:hover { background: #5a885e; }
.submit-btn:disabled { background: #B3CCAE; cursor: not-allowed; }
.error-msg { color: #BB4E58; font-weight: 600; font-size: 16px; margin-bottom: 10px; }
.success-msg { color: #689D6D; font-weight: 600; font-size: 16px; margin-bottom: 20px; padding: 15px; background: #e8f5e9; border-radius: 10px; }
.success-msg a { color: #689D6D; text-decoration: underline; margin-top: 10px; display: block; font-weight: bold;}
</style>