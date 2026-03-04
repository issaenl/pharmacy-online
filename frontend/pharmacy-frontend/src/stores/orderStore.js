import { defineStore } from 'pinia';
import { ref } from 'vue';
import api from '@/api/api';
import { useAuthStore } from './authStore';
import { useCartStore } from './cartStore';
import { useToast } from 'vue-toast-notification';

export const useOrderStore = defineStore('order', () => {
  const selectedPharmacy = ref(null);
  const authStore = useAuthStore();
  const cartStore = useCartStore();
  const toast = useToast({ position: 'bottom-right' });

  const setPharmacy = async (pharmacy) => {
    selectedPharmacy.value = pharmacy;
    if (authStore.token) {
      await api.put(`/Cart/pharmacy/${pharmacy.id}`);
      await cartStore.loadCart();
    } else {
      localStorage.setItem('selectedPharmacy', JSON.stringify(pharmacy));
      await cartStore.recalculatePrices(pharmacy.id);
    }
    toast.success(`Выбрана ${pharmacy.name}`);
  };

  const checkout = async () => {
    if (!authStore.token) {
      toast.info('Для оформления заказа нужно войти в аккаунт');
      return;
    }
    if (!selectedPharmacy.value) {
      toast.error('Аптека не выбрана');
      return;
    }

    try {
      const response = await api.post('/Orders/checkout');
      toast.success(response.data.message || 'Бронь успешно оформлена!', { duration: 5000 });
      cartStore.items = [];
      selectedPharmacy.value = null;
      window.location.href = '/';
    } catch (error) {
      if (error.response && error.response.data) {
        toast.error(error.response.data);
      } else {
        toast.error('Произошла ошибка при оформлении брони.');
      }
      console.error(error);
    }
  };

  const quickCheckout = async (productId, pharmacyId, quantity) => {
    try {
      const payload = { productId, pharmacyId, quantity };
      const response = await api.post('/Orders/quick-checkout', payload);
      toast.success(response.data.message || 'Бронь успешно оформлена!');
      return true;
    } catch (error) {
      if (error.response && error.response.data) {
        toast.error(error.response.data);
      } else {
        toast.error('Произошла ошибка при оформлении брони.');
      }
      return false;
    }
  };

  return { selectedPharmacy, setPharmacy, checkout, quickCheckout };
});