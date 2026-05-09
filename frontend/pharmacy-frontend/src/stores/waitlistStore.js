import { defineStore } from 'pinia';
import { ref } from 'vue';
import api from '@/api/api';
import { useToast } from 'vue-toast-notification';

export const useWaitlistStore = defineStore('waitlist', () => {
  const items = ref([]);
  const toast = useToast({ position: 'bottom-right' });
  const isLoading = ref(false);

  const fetchWaitlist = async () => {
    isLoading.value = true;
    try {
      const response = await api.get('/Waitlist');
      items.value = response.data;
    } catch (error) {
      console.error('Ошибка загрузки листа ожидания', error);
      toast.error('Не удалось загрузить лист ожидания');
    } finally {
      isLoading.value = false;
    }
  };

  const addToWaitlist = async (productId, district) => {
    try {
      await api.post('/Waitlist', { productId, district });
      toast.success('Товар добавлен в лист ожидания. Мы пришлем уведомление!');
      await fetchWaitlist();
      return true;
    } catch (error) {
      const errorMsg = error.response?.data?.message || error.response?.data || 'Ошибка добавления';
      toast.error(errorMsg);
      return false;
    }
  };

  const removeFromWaitlist = async (id) => {
    try {
      await api.delete(`/Waitlist/${id}`);
      items.value = items.value.filter(item => item.id !== id);
      toast.success('Товар удален из листа ожидания');
    } catch (error) {
      toast.error('Ошибка при удалении');
    }
  };

  const clearWaitlist = async () => {
    try {
      await api.delete('/Waitlist/clear');
      items.value = [];
      toast.success('Лист ожидания полностью очищен');
    } catch (error) {
      toast.error('Ошибка при очистке');
    }
  };

  return { items, isLoading,
    fetchWaitlist, addToWaitlist, removeFromWaitlist, clearWaitlist };
});