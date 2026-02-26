import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import api from '@/api/api';
import { useAuthStore } from './authStore';
import { useToast } from 'vue-toast-notification';

export const useCartStore = defineStore('cart', () => {
  const items = ref([]);
  const selectedPharmacy = ref(null);
  const authStore = useAuthStore();
  const toast = useToast({ position: 'bottom-right' });

  const loadCart = async () => {
    if (authStore.token) {
      try {
        const response = await api.get('/Cart');
        items.value = response.data.items; 
        if (response.data.pharmacy) {
            selectedPharmacy.value = response.data.pharmacy;
        }
      } catch (error) {
        console.error("Ошибка загрузки корзины с сервера:", error);
      }
    } else {
      const localCart = localStorage.getItem('cart');
      if (localCart) items.value = JSON.parse(localCart);
    }
  };

  const setPharmacy = async (pharmacy) => {
    selectedPharmacy.value = pharmacy;
    if(authStore.token) {
      await api.put(`/Cart/pharmacy/${pharmacy.id}`);
      await loadCart();
    }
    else {
      localStorage.setItem('selectedPharmacy', JSON.stringify(pharmacy));
        if (items.value.length > 0) {
          try {
            const requestData = items.value.map(i => ({ productId: i.productId, quantity: i.quantity }));
            const response = await api.post(`/Cart/recalculate/${pharmacy.id}`, requestData);
            items.value = response.data; 
            saveLocalCart();
          } 
          catch (e) {
            console.error("Ошибка пересчета цен:", e);
          }
        }
      toast.success(`Выбрана ${pharmacy.name}`);
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

  const checkout = async () => {
    if(!authStore.token){
      toast.info('Для оформления заказа нужно войти в аккаунт');
      return;
    }
    if(!selectedPharmacy.value){
      toast.error('Аптека не выбрана');
      return;
    }

    try{
      const response = await api.post('/Orders/checkout');
      toast.success(response.data.message || 'Бронь успешно оформлена!', { duration: 5000 } );
      items.value = [];
      selectedPharmacy.value = null;
      window.location.href = '/';
    }
    catch (error){
      if (error.response && error.response.data) {
             toast.error(error.response.data); 
        } else {
             toast.error('Произошла ошибка при оформлении брони.');
        }
        console.error(error);
    }
  }

  const totalItems = computed(() => items.value.reduce((sum, item) => sum + item.quantity, 0));
  const totalPrice = computed(() => items.value.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0));

  return { items, selectedPharmacy, totalItems, totalPrice,
    loadCart, addToCart, updateQuantity, removeFromCart, clearCart, syncCart, setPharmacy, checkout};
});