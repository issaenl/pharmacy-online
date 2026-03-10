import { defineStore } from 'pinia';
import { ref } from 'vue';
import api from '@/api/api';
import { useAuthStore } from './authStore';
import { useToast } from 'vue-toast-notification';

export const useFavoriteStore = defineStore('favorite', () => {
  const items = ref([]);
  const authStore = useAuthStore();
  const toast = useToast({ position: 'bottom-right' });

  const loadFavorites = async () => {
    if (authStore.token) {
      try {
        const response = await api.get('/Favorites');
        items.value = response.data;
      } 
      catch (error) {
      }
    } else {
      const localFavorites = localStorage.getItem('favorites');
      if (localFavorites) items.value = JSON.parse(localFavorites);
    }
  };

  const saveLocalFavorites = () => {
    if (!authStore.token) {
      localStorage.setItem('favorites', JSON.stringify(items.value));
    }
  };

  const isFavorite = (productId) => {
    return items.value.some(item => item.productId === productId);
  };

  const toggleFavorite = async (product) => {
    const id = product.id || product.productId;
    const exists = isFavorite(id);

    if (exists) {
      items.value = items.value.filter(i => i.productId !== id);
      if (authStore.token) {
        try {
          await api.delete(`/Favorites/${id}`);
        } 
        catch (e) {
        }
      } else {
        saveLocalFavorites();
      }
      toast.info('Товар удален из избранного');
    } else {
      items.value.push({
        productId: id,
        productName: product.name || product.productName,
        price: product.minPrice || product.price || product.unitPrice,
        pictureUrl: product.pictureUrl || product.imageUrl,
        dosageForm: product.dosageForm
      });

      if (authStore.token) {
        try {
          await api.post(`/Favorites/${id}`);
        } 
        catch (e) {
        }
      } else {
        saveLocalFavorites();
      }
      toast.success('Товар добавлен в избранное');
    }
  };

  const syncFavorites = async () => {
    const localFavorites = localStorage.getItem('favorites');
    if (localFavorites) {
      const parsedLocal = JSON.parse(localFavorites);
      if (parsedLocal.length > 0) {
        for (const item of parsedLocal) {
          try {
            await api.post(`/Favorites/${item.productId}`);
          } 
          catch (e) {
          }
        }
      }
      localStorage.removeItem('favorites');
    }
    await loadFavorites();
  };

  const clearFavorites = async () => {
    items.value = [];
    if (!authStore.token) {
      localStorage.removeItem('favorites');
    }
  };

  return { items, loadFavorites, isFavorite, toggleFavorite, syncFavorites, clearFavorites };
});