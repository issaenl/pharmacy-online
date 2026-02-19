<template>
  <div class="profile-info-component">
    <div class="section-header">
      <h2>Личные данные</h2>
      <button v-if="!isEditingProfile" @click="startEditingProfile" class="edit-text-btn">Изменить данные</button>
    </div>
    
    <div v-if="!isEditingProfile && authStore.user" class="info-card">
      <div class="info-group">
        <label>Имя</label>
        <div class="info-value">{{ authStore.user.firstName }}</div>
      </div>
      <div class="info-group">
        <label>Фамилия</label>
        <div class="info-value">{{ authStore.user.lastName || 'Не указана' }}</div>
      </div>
      <div class="info-group">
        <label>Телефон</label>
        <div class="info-value">{{ authStore.user.phone }}</div>
      </div>
      <div class="info-group">
        <label>Email</label>
        <div class="info-value">{{ authStore.user.email || 'Не указан' }}</div>
      </div>
    </div>

    <form v-else-if="isEditingProfile" @submit.prevent="saveProfile" class="info-card edit-form">
      <div class="form-group">
        <label>Имя</label>
        <input type="text" v-model="editForm.firstName" required />
      </div>
      <div class="form-group">
        <label>Фамилия</label>
        <input type="text" v-model="editForm.lastName" placeholder="Иванов" />
      </div>
      <div class="form-group">
        <label>Телефон</label>
        <input type="text" v-model="editForm.phone" @input="handlePhoneInput" required />
      </div>
      <div class="form-group">
        <label>Email</label>
        <input type="email" v-model="editForm.email" placeholder="example@mail.ru" />
      </div>
      <div class="form-actions">
        <button type="submit" class="save-btn" :disabled="isLoading">Сохранить</button>
        <button type="button" @click="isEditingProfile = false" class="cancel-btn">Отмена</button>
      </div>
    </form>

    <div class="section-header security-header">
      <h2>Безопасность</h2>
      <button v-if="!isChangingPassword" @click="isChangingPassword = true" class="edit-text-btn">Изменить пароль</button>
    </div>

    <form v-if="isChangingPassword" @submit.prevent="savePassword" class="info-card edit-form password-form">
      <div class="form-group full-width">
        <label>Текущий пароль</label>
        <input type="password" v-model="passwordForm.oldPassword" required />
      </div>
      <div class="form-group">
        <label>Новый пароль</label>
        <input type="password" v-model="passwordForm.newPassword" required minlength="8" />
      </div>
      <div class="form-group">
        <label>Повторите новый пароль</label>
        <input type="password" v-model="passwordForm.confirmPassword" required minlength="8" />
      </div>
      
      <p v-if="passwordError" class="error-msg full-width">{{ passwordError }}</p>
      <p v-if="passwordSuccess" class="success-msg full-width">Пароль успешно изменен!</p>

      <div class="form-actions full-width">
        <button type="submit" class="save-btn" :disabled="isLoading">Обновить пароль</button>
        <button type="button" @click="cancelPasswordChange" class="cancel-btn">Отмена</button>
      </div>
    </form>
    
    <div class="section-header security-header danger-zone">
      <h2>Удаление аккаунта</h2>
    </div>
    <div class="info-card danger-card">
      <div class="info-group">
        <p class="danger-text">Внимание! Это действие необратимо. Все ваши данные и история заказов будут удалены навсегда.</p>
      </div>
      <div class="form-actions">
        <button @click="confirmDelete" class="delete-btn" :disabled="isLoading">
          Навсегда удалить аккаунт
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useAuthStore } from '@/stores/authStore';

const authStore = useAuthStore();

const isEditingProfile = ref(false);
const isChangingPassword = ref(false);
const isLoading = ref(false);

const editForm = ref({ firstName: '', lastName: '', phone: '', email: '' });
const passwordForm = ref({ oldPassword: '', newPassword: '', confirmPassword: '' });

const passwordError = ref('');
const passwordSuccess = ref(false);

const getErrorMessage = (error) => {
  const data = error.response?.data;
  
  if (data && typeof data === 'object' && data.errors) {
    const messages = Object.values(data.errors).flat();
    return messages[0] || 'Ошибка валидации данных';
  }
  
  if (data && data.message) {
    return data.message;
  }
  
  if (typeof data === 'string') {
    return data;
  }

  return 'Неизвестная ошибка сервера.';
};

const startEditingProfile = () => {
  editForm.value = { 
    firstName: authStore.user.firstName, 
    lastName: authStore.user.lastName || '', 
    phone: authStore.user.phone, 
    email: authStore.user.email || '' 
  };
  isEditingProfile.value = true;
};

const saveProfile = async () => {
  isLoading.value = true;
  try {
    await authStore.updateProfile(editForm.value);
    isEditingProfile.value = false;
  } catch (error) {
    alert('Ошибка при сохранении данных: ' + getErrorMessage(error));
  } finally {
    isLoading.value = false;
  }
};

const cancelPasswordChange = () => {
  isChangingPassword.value = false;
  passwordError.value = '';
  passwordSuccess.value = false;
  passwordForm.value = { oldPassword: '', newPassword: '', confirmPassword: '' };
};

const savePassword = async () => {
  passwordError.value = '';
  passwordSuccess.value = false;

  if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
    passwordError.value = 'Новые пароли не совпадают!';
    return;
  }

  isLoading.value = true;
  try {
    await authStore.changePassword({
      oldPassword: passwordForm.value.oldPassword,
      newPassword: passwordForm.value.newPassword
    });
    passwordSuccess.value = true;
    setTimeout(() => cancelPasswordChange(), 2000);
  } catch (error) {
    passwordError.value = getErrorMessage(error);
  } finally {
    isLoading.value = false;
  }
};

const confirmDelete = async () => {
  const isConfirmed = confirm("Вы абсолютно уверены, что хотите удалить аккаунт? Это действие нельзя отменить.");
  
  if (isConfirmed) {
    isLoading.value = true;
    try {
      await authStore.deleteAccount(); 
      window.location.href = '/';
    } catch (error) {
      alert("Ошибка при удалении аккаунта: " + (error.response?.data || error.message));
    } finally {
      isLoading.value = false;
    }
  }
};

const handlePhoneInput = (event) => {
  let inputDigits = event.target.value.replace(/[^\d]/g, '');
  
  if (inputDigits) {
    editForm.value.phone = '+' + inputDigits;
  } else {
    editForm.value.phone = '';
  }
};
</script>

<style scoped>
    .section-header { 
        display: flex; 
        justify-content: space-between; 
        align-items: center; 
        margin-bottom: 20px; 
    }

    .section-header h2 { 
        margin: 0; 
        font-size: 28px; 
        color: #000; 
    }

    .security-header { 
        margin-top: 40px; 
    }

    .edit-text-btn { 
        background: none; 
        border: none; 
        color: #689D6D; 
        font-size: 16px; 
        font-weight: 600; 
        cursor: pointer; 
        padding: 0; 
        font-family: var(--main-font); 
    }

    .edit-text-btn:hover { 
        text-decoration: underline; 
    }

    .info-card { 
        background: white; 
        padding: 30px; 
        border-radius: 20px; 
        box-shadow: 0 4px 12px rgba(0,0,0,0.05); 
        display: grid; 
        grid-template-columns: 1fr 1fr; 
        gap: 20px; 
    }

    .info-group { 
        display: flex; 
        flex-direction: column; 
        gap: 8px; 
    }

    .info-group label { 
        font-size: 14px; 
        color: #888; 
    }

    .info-value { 
        font-size: 18px; 
        font-weight: 500; 
        color: #000; 
        padding: 12px; 
        background: #F9F9F9; 
        border-radius: 10px; 
    }

    .edit-form .form-group input { 
        width: 100%; 
        padding: 12px; 
        border: 1px solid #ddd; 
        border-radius: 10px; 
        font-family: var(--main-font); 
        font-size: 16px; 
        background: #F9F9F9; 
        transition: 0.2s; 
        box-sizing: border-box;
    }

    .edit-form .form-group input:focus { 
        border-color: #689D6D; 
        outline: none; 
        background: #fff; 
    }

    .full-width { 
        grid-column: 1 / -1; 
    }

    .form-actions { 
        display: flex; 
        gap: 15px; 
        grid-column: 1 / -1; 
        margin-top: 10px; 
    }

    .save-btn { 
        background: #689D6D; 
        color: white; 
        border: none; 
        padding: 12px 25px; 
        border-radius: 10px; 
        font-weight: 600; 
        font-size: 16px; 
        cursor: pointer; 
        font-family: var(--main-font); 
    }

    .save-btn:hover { 
        background: #5a885e; 
    }

    .cancel-btn { 
        background: #f0f0f0; 
        color: #555; 
        border: none; 
        padding: 12px 25px; 
        border-radius: 10px; 
        font-weight: 600; 
        font-size: 16px; 
        cursor: pointer; 
        font-family: var(--main-font); 
    }
    
    .cancel-btn:hover { 
        background: #e4e4e4; 
    }

    .error-msg { 
        color: #BB4E58; 
        font-size: 14px; 
        margin: 0; 
    }

    .success-msg { 
        color: #689D6D; 
        font-size: 14px; 
        margin: 0; 
        font-weight: 600; 
    }

    .danger-zone { 
        margin-top: 50px; 
        padding-top: 20px;
    }

    .danger-card { 
        border: 1px solid #ffcccc; 
        background: #fffafb; 
    }

    .danger-text { 
        font-size: 14px; 
        color: #BB4E58; 
        margin: 0; 
    }

    .delete-btn { 
        background: #BB4E58; 
        color: white; 
        border: none; 
        padding: 12px 25px; 
        border-radius: 10px; 
        font-weight: 600; 
        font-size: 16px; 
        cursor: pointer; 
        font-family: var(--main-font); 
        transition: 0.2s;
    }

    .delete-btn:hover { 
        background: #a33b45; 
    }
    
    .delete-btn:disabled { 
        background: #dca7ab; 
        cursor: not-allowed; 
    }

    @media (max-width: 768px) {
        .info-card { 
            grid-template-columns: 1fr;
        }
    }
</style>