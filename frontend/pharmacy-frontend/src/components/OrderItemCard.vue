<template>
  <div class="order-card" :class="{ expanded: isExpanded }">
    <div class="order-header" @click="toggleExpand">
      
      <div class="header-top">
        <div class="header-left">
          <div class="order-id">Заказ №{{ order.id }}</div>
          <div class="order-date">{{ formatDate(order.orderDate) }}</div>
        </div>
        
        <div class="header-center">
          <div class="order-pharmacy">{{ order.pharmacyName }}</div>
          <div class="order-status" :class="statusClass">{{ statusText }}</div>
        </div>

        <div class="header-right">
          <div class="order-price">{{ order.totalPrice.toFixed(2) }} р.</div>
          <button class="expand-btn">
            <svg v-if="!isExpanded" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"></polyline></svg>
            <svg v-else width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="18 15 12 9 6 15"></polyline></svg>
          </button>
        </div>
      </div>

      <div class="order-actions" @click.stop>
        <button 
          v-if="order.status === 2 && !order.hasReview" 
          class="action-btn review-btn" 
          @click.stop="$emit('review', order.id)">
          Оценить аптеку
        </button>

        <span v-if="order.status === 2 && order.hasReview" class="reviewed-badge">
          ✓ Отзыв оставлен
        </span>

        <button class="action-btn repeat-btn" @click.stop="repeatOrder">Повторить заказ</button>
        <button v-if="order.status === 0" class="action-btn cancel-btn" @click.stop="cancelOrder">Отменить заказ</button>
      </div>

    </div>

    <div class="order-details" v-if="isExpanded" @click.stop>
      <div class="details-info">
        <p><strong>Адрес самовывоза:</strong> {{ order.pharmacyAddress }}</p>
      </div>

      <div class="items-list">
        <div v-for="item in order.items" :key="item.productId" class="order-item-row">
          <span class="item-name">{{ item.productName }}</span>
          <span class="item-qty-price">{{ item.quantity }} шт. x {{ item.price.toFixed(2) }} р.</span>
          <span class="item-total">{{ item.totalPrice.toFixed(2) }} р.</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useCartStore } from '@/stores/cartStore';

const props = defineProps({
  order: {
    type: Object,
    required: true
  }
});

const emit = defineEmits(['cancel', 'review']);
const cartStore = useCartStore();
const isExpanded = ref(false);

const toggleExpand = () => {
  isExpanded.value = !isExpanded.value;
};

const formatDate = (dateString) => {
  const options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
  return new Date(dateString).toLocaleDateString('ru-RU', options);
};

const statusText = computed(() => {
  const statuses = {
    0: 'Ожидает сборки',
    1: 'Готов к выдаче',
    2: 'Выполнен',
    3: 'Отменен',
    4: 'Истек срок',
    5: 'Ошибка'
  };
  return statuses[props.order.status] || 'Неизвестно';
});

const statusClass = computed(() => {
  const classes = {
    0: 'status-pending',
    1: 'status-ready',
    2: 'status-completed',
    3: 'status-cancelled',
    4: 'status-expired'
  };
  return classes[props.order.status] || '';
});

const repeatOrder = async () => {
  for (const item of props.order.items) {
    await cartStore.addToCart({ id: item.productId, name: item.productName, price: item.price }, item.quantity);
  }
  cartStore.toast.success('Товары добавлены в корзину!');
};

const cancelOrder = () => {
  if (confirm('Вы уверены, что хотите отменить этот заказ?')) {
    emit('cancel', props.order.id);
  }
};
</script>

<style scoped>
    .order-card {
        display: block;
        width: 100%; 
        box-sizing: border-box;
        background: white;
        border-radius: 16px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.04);
        margin-bottom: 15px;
        overflow: hidden;
        transition: 0.3s;
        font-family: var(--main-font);
    }

    .order-card.expanded {
        box-shadow: 0 4px 15px rgba(0,0,0,0.08);
    }

    .order-header {
        display: flex;
        flex-direction: column;
        padding: 20px;
        cursor: pointer;
        background: #fff;
        gap: 15px;
    }

    .order-header:hover {
        background: #FAFAFA;
    }

    .header-top {
        display: flex;
        justify-content: space-between;
        align-items: center;
        width: 100%;
    }

    .header-left { flex: 1; }

    .order-id {
        font-weight: 700;
        font-size: 20px;
        color: #333;
        margin-bottom: 5px;
    }

    .order-date { font-size: 14px; color: #888; }

    .header-center {
        flex: 1;
        text-align: center;
    }

    .order-pharmacy {
        font-size: 14px;
        color: #000;
        margin-bottom: 5px;
    }

    .order-status {
        display: inline-block;
        padding: 6px 10px;
        border-radius: 6px;
        font-size: 16px;
        font-weight: 600;
    }

    .status-pending { background: #fcf1d6; color: #F3C301; }
    .status-ready { background: #E8F4EA; color: #689D6D; }
    .status-completed { background: #F0F0F0; color: #333; }
    .status-cancelled { background: #FDE8E8; color: #BB4E58; }
    .status-expired { background: #F5F5F5; color: #888; }

    .header-right {
        flex: 1;
        display: flex;
        justify-content: flex-end;
        align-items: center;
        gap: 15px;
    }

    .order-price {
        font-weight: 700;
        font-size: 20px;
        color: #000;
    }

    .expand-btn {
        background: none;
        border: none;
        color: #888;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .order-actions {
        display: flex;
        gap: 15px;
        justify-content: flex-end;
        align-items: center;
        width: 100%;
        padding-top: 15px;
        border-top: 1px dashed #eee;
    }

    .action-btn {
        padding: 10px 20px;
        border-radius: 8px;
        font-size: 16px;
        font-weight: 600;
        cursor: pointer;
        font-family: var(--main-font);
        transition: 0.2s;
    }

    .repeat-btn {
        background: var(--primary-color);
        color: white;
        border: none;
    }
    .repeat-btn:hover { filter: brightness(0.9); }

    .cancel-btn {
        background: transparent;
        border: 1px solid #BB4E58 !important;
        color: #BB4E58;
    }
    .cancel-btn:hover { background: #BB4E58; color: white; }

    .review-btn {
        background-color: white;
        color: var(--primary-color);
        border: 1px solid var(--primary-color) !important; 
    }
    .review-btn:hover {
        background: var(--primary-color);
        color: white;
    }

    .reviewed-badge {
        color: #888;
        font-size: 16px;
        font-weight: 500;
        display: flex;
        align-items: center;
        gap: 5px;
    }

    .order-details {
        padding: 0 20px 20px 20px;
        background: #fff;
    }

    .details-info { margin: 0 0 15px 0; font-size: 16px; color: #000; }

    .items-list {
        background: #FAFAFA;
        border-radius: 10px;
        padding: 15px;
        border: 1px solid #eee;
    }

    .order-item-row {
        display: flex;
        justify-content: space-between;
        padding: 8px 0;
        border-bottom: 1px dashed #e0e0e0;
        font-size: 16px;
    }

    .order-item-row:last-child { border-bottom: none; padding-bottom: 0; }

    .item-name { flex: 2; color: #000; font-weight: 500; }
    .item-qty-price { flex: 1; color: #888; text-align: center; }
    .item-total { flex: 1; color: #000; font-weight: 600; text-align: right; }

    @media (max-width: 768px) {
        .header-top { flex-direction: column; align-items: stretch; gap: 10px; }
        .header-left, .header-center { width: 100%; text-align: left; }
        .header-right { width: 100%; justify-content: space-between; }
        .order-actions { flex-direction: column; align-items: stretch; gap: 10px; }
        .action-btn { width: 100%; text-align: center; }
        .order-item-row { flex-direction: column; gap: 5px; }
        .item-qty-price, .item-total { text-align: left; }
    }
</style>