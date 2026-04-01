import { defineStore } from 'pinia';
import { ref } from 'vue';
import api from '@/api/api';

import { useCartStore } from './cartStore';
import { useFavoriteStore } from './favoriteStore';

const extractErrorMessage = (error) => {
  if (error.response && error.response.data) {
    const data = error.response.data;
    if (data.errors && typeof data.errors === 'object') {
      const firstField = Object.keys(data.errors)[0];
      return data.errors[firstField][0];
    }
    if (typeof data === 'string') return data;
    if (data.title) return data.title;
  }
  return error.message || 'Произошла непредвиденная ошибка';
};

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || null);
  const user = ref(JSON.parse(localStorage.getItem('user')) || null);

  if (token.value) {
    api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`;
  }

  const fetchUser = async () => {
    if (!token.value) return;
    try {
      const response = await api.get('/Auth/me');
      user.value = response.data;
      localStorage.setItem('user', JSON.stringify(user.value));
    } catch (error) {
      console.error("Ошибка обновления профиля", error);
      if (error.response?.status === 401 || error.response?.status === 404) {
        logout();
      }
    }
  };

  const login = async (loginCredential, password) => {
    try {
      const response = await api.post('/Auth/login', { login: loginCredential, password });
      
      token.value = response.data.token;
      user.value = response.data.user;
      
      localStorage.setItem('token', token.value);
      localStorage.setItem('user', JSON.stringify(user.value));
      
      api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`;

      try {
        const cartStore = useCartStore();
        const favoriteStore = useFavoriteStore();
        await cartStore.syncCart();
        await favoriteStore.syncFavorites();
      } catch (error) {
        console.error(error);
      }
    } catch (error) {
      throw extractErrorMessage(error); 
    }
  };

  const register = async (userData) => {
    try {
      await api.post('/Auth/register', userData);
      await login(userData.phone, userData.password);
    } catch (error) {
      throw extractErrorMessage(error);
    }
  };

  const logout = () => {
    token.value = null;
    user.value = null;
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    delete api.defaults.headers.common['Authorization'];

    try {
      import('@/stores/cartStore').then(({ useCartStore }) => {
        const cartStore = useCartStore();
        const favoriteStore = useFavoriteStore();
        cartStore.items = [];
        favoriteStore.items = [];
      });
    } catch (error) {
      console.error('Ошибка при очистке корзины:', error);
    }
  };

  const updateProfile = async (userData) => {
    try {
      const response = await api.put('/Auth/update-profile', userData);
      user.value = response.data; 
      localStorage.setItem('user', JSON.stringify(user.value));
    } catch (error) {
      throw extractErrorMessage(error);
    }
  };

  const changePassword = async (passwords) => {
    try {
      await api.put('/Auth/change-password', passwords);
    } catch (error) {
      throw extractErrorMessage(error);
    }
  };

  const deleteAccount = async () => {
    await api.delete('/Auth/delete');
    logout();
  };

  return { 
    token, user, 
    login, register, logout, updateProfile, changePassword, deleteAccount, fetchUser
  };
});