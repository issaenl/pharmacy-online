<template>
  <div class="profile-orders-component">
    <div class="section-header">
      <h2>История заказов</h2>
    </div>
    
    <div v-if="isLoading" class="loading-state">
      <p>Загрузка заказов...</p>
    </div>

    <div v-else-if="orders.length === 0" class="empty-state">
      <img src="/assets/Cart.svg" alt="Пусто" class="empty-icon">
      <p>У вас пока нет оформленных заказов.</p>
      <router-link to="/full-catalog" class="go-catalog-btn">Перейти в каталог</router-link>
    </div>

    <div v-else class="orders-list">
      <OrderItemCard 
        v-for="order in orders" 
        :key="order.id" 
        :order="order"
        @cancel="handleCancelOrder" 
      />
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import api from '@/api/api';
import { useToast } from 'vue-toast-notification';
import OrderItemCard from '@/components/OrderItemCard.vue'; 

const orders = ref([]);
const isLoading = ref(true);
const toast = useToast({ position: 'bottom-right' });

const fetchOrders = async () => {
  try {
    isLoading.value = true;
    const response = await api.get('/Orders/my-orders');
    orders.value = response.data;
  } catch (error) {
    console.error('Ошибка загрузки заказов:', error);
    toast.error('Не удалось загрузить историю заказов');
  } finally {
    isLoading.value = false;
  }
};


const handleCancelOrder = async (orderId) => {
  try {
    const response = await api.put(`/Orders/${orderId}/cancel`);
    toast.success(response.data.message || 'Заказ отменен');
    await fetchOrders(); 
  } catch (error) {
    if (error.response?.data) {
      toast.error(error.response.data);
    } else {
      toast.error('Ошибка при отмене заказа');
    }
  }
};

onMounted(() => {
  fetchOrders();
});
</script>

<style scoped>
    .section-header {
        margin-bottom: 20px;
    }

    .section-header h2 {
        margin: 0;
        font-size: 28px;
        color: #000;
    }

    .loading-state, .empty-state {
        text-align: center;
        background: white;
        padding: 50px;
        border-radius: 20px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.05);
    }

    .empty-icon {
        width: 64px;
        height: 64px;
        opacity: 0.3;
        margin-bottom: 20px;
    }

    .empty-state p {
        font-size: 18px;
        color: #666;
        margin-bottom: 20px;
    }

    .go-catalog-btn {
        display: inline-block;
        background: var(--primary-color);
        color: white;
        text-decoration: none;
        padding: 12px 25px;
        border-radius: 10px;
        font-weight: 600;
    }

    .profile-orders-component {
        width: 100%;
    }

    .orders-list {
        display: flex;
        flex-direction: column;
        width: 100%;
        align-items: stretch;
        gap: 10px;
    }

</style>