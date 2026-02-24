import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import api from '@/api/api';
import { useAuthStore } from './authStore';
import { useToast } from 'vue-toast-notification';

export const useCartStore = defineStore('cart', () => {
  const items = ref([]);
  const authStore = useAuthStore();
  const toast = useToast({ position: 'bottom-right' });

  const loadCart = async () => {
    if (authStore.token) {
      try {
        const response = await api.get('/Cart');
        items.value = response.data.items; 
      } catch (error) {
        console.error("Ошибка загрузки корзины с сервера:", error);
      }
    } else {
      const localCart = localStorage.getItem('cart');
      if (localCart) items.value = JSON.parse(localCart);
    }
  };

  const saveLocalCart = () => {
    if (!authStore.token) {
      localStorage.setItem('cart', JSON.stringify(items.value));
    }
  };

  const addToCart = async (product, quantity = 1) => {
    const existing = items.value.find(i => i.productId === product.id);
    
    if (existing) {
      existing.quantity += quantity;
    } else {
      items.value.push({
        productId: product.id,
        productName: product.name, 
        unitPrice: product.price,  
        imageUrl: product.imageUrl,
        quantity: quantity
      });
    }

    if (authStore.token) {
      await api.post('/Cart/add', { productId: product.id, quantity });
    } else {
      saveLocalCart();
    }
    toast.success('Товар добавлен в корзину');
  };

  const updateQuantity = async (productId, newQuantity) => {
    if (newQuantity < 1) return;
    
    const item = items.value.find(i => i.productId === productId);
    if (item) {
      item.quantity = newQuantity;
      if (authStore.token) {
        await api.put('/Cart/update', { productId, quantity: newQuantity });
      } else {
        saveLocalCart();
      }
    }
  };

  const removeFromCart = async (productId) => {
    items.value = items.value.filter(i => i.productId !== productId);
    if (authStore.token) {
      await api.delete(`/Cart/remove/${productId}`);
    } else {
      saveLocalCart();
    }
    toast.info('Товар удален из корзины');
  };

  const clearCart = async () => {
    items.value = [];
    if (authStore.token) {
      await api.delete('/Cart/clear');
    } else {
      localStorage.removeItem('cart');
    }
  };

  const syncCart = async () => {
    const localCart = localStorage.getItem('cart');
    if (localCart) {
      const parsedLocal = JSON.parse(localCart);
      if (parsedLocal.length > 0) {
        const syncData = parsedLocal.map(i => ({ productId: i.productId, quantity: i.quantity }));
        await api.post('/Cart/sync', syncData);
      }
      localStorage.removeItem('cart'); 
    }
    await loadCart();
  };

  const totalItems = computed(() => items.value.reduce((sum, item) => sum + item.quantity, 0));
  const totalPrice = computed(() => items.value.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0));

  return { items, loadCart, addToCart, updateQuantity, removeFromCart, clearCart, syncCart, totalItems, totalPrice };
});