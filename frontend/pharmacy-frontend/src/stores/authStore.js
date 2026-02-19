import { defineStore } from 'pinia';
import { ref } from 'vue';
import api from '@/api/api';

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || null);
  const user = ref(JSON.parse(localStorage.getItem('user')) || null);

  if (token.value) {
    api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`;
  }

  const login = async (phone, password) => {
    const response = await api.post('/Auth/login', { phone, password });
    
    token.value = response.data.token;
    user.value = response.data.user;
    
    localStorage.setItem('token', token.value);
    localStorage.setItem('user', JSON.stringify(user.value));
    
    api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`;
  };

  const register = async (userData) => {
    await api.post('/Auth/register', userData);
    await login(userData.phone, userData.password);
  };

  const logout = () => {
    token.value = null;
    user.value = null;
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    delete api.defaults.headers.common['Authorization'];
  };

  const updateProfile = async (userData) => {
    const response = await api.put('/Auth/update-profile', userData);
    user.value = response.data; 
    localStorage.setItem('user', JSON.stringify(user.value));
  };

  const changePassword = async (passwords) => {
    await api.put('/Auth/change-password', passwords);
  };

  const deleteAccount = async () => {
    await api.delete('/Auth/delete');
    logout();
  };

  return { token, user, login, register, logout, updateProfile, changePassword, deleteAccount };
});