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
        @review="openReviewModal" 
      />
    </div>

    <Modal 
      :show="showReviewModal" 
      title="Оставить отзыв об аптеке" 
      @close="closeReviewModal"
    >
      <div class="review-form">
        <div class="rating-block">
          <label>Ваша оценка:</label>
          <div class="stars">
            <span 
              v-for="n in 5" 
              :key="n" 
              class="star" 
              :class="{ 'active': n <= reviewForm.rating }"
              @click="reviewForm.rating = n"
            >
              ★
            </span>
          </div>
        </div>

        <div class="comment-block">
          <label>Комментарий (необязательно):</label>
          <textarea 
            v-model="reviewForm.comment" 
            rows="4" 
            placeholder="Расскажите, как все прошло..."
          ></textarea>
        </div>

        <div class="modal-actions">
          <button class="cancel-btn" @click="closeReviewModal">Отмена</button>
          <button 
            class="submit-btn" 
            :disabled="!reviewForm.rating || isSubmitting"
            @click="submitReview"
          >
            {{ isSubmitting ? 'Отправка...' : 'Отправить отзыв' }}
          </button>
        </div>
      </div>
    </Modal>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import api from '@/api/api';
import { useToast } from 'vue-toast-notification';
import OrderItemCard from '@/components/OrderItemCard.vue'; 
import Modal from '@/components/admin/Modal.vue'; 

const orders = ref([]);
const isLoading = ref(true);
const toast = useToast({ position: 'bottom-right' });

const showReviewModal = ref(false);
const selectedOrderId = ref(null);
const isSubmitting = ref(false);
const reviewForm = ref({
  rating: 0,
  comment: ''
});

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

const openReviewModal = (orderId) => {
  selectedOrderId.value = orderId;
  reviewForm.value = { rating: 0, comment: '' };
  showReviewModal.value = true;
};

const closeReviewModal = () => {
  showReviewModal.value = false;
  selectedOrderId.value = null;
};

const submitReview = async () => {
  if (!reviewForm.value.rating) return;
  
  isSubmitting.value = true;
  try {
    const payload = {
      orderId: selectedOrderId.value,
      rating: reviewForm.value.rating,
      comment: reviewForm.value.comment
    };
    
    const response = await api.post('/Reviews', payload);
    toast.success(response.data.message || 'Отзыв успешно отправлен!');
    closeReviewModal();
  } catch (error) {
    toast.error(error.response?.data || 'Ошибка при отправке отзыва');
  } finally {
    isSubmitting.value = false;
  }
};

onMounted(() => {
  fetchOrders();
});
</script>

<style scoped>
    .section-header { margin-bottom: 20px; }
    .section-header h2 { margin: 0; font-size: 28px; color: #000; }

    .loading-state, .empty-state {
        text-align: center;
        background: white;
        padding: 50px;
        border-radius: 20px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.05);
    }

    .empty-icon { width: 64px; height: 64px; opacity: 0.3; margin-bottom: 20px; }
    .empty-state p { font-size: 18px; color: #666; margin-bottom: 20px; }

    .go-catalog-btn {
        display: inline-block;
        background: var(--primary-color);
        color: white;
        text-decoration: none;
        padding: 12px 25px;
        border-radius: 10px;
        font-weight: 600;
    }

    .profile-orders-component { width: 100%; }

    .orders-list {
        display: flex;
        flex-direction: column;
        width: 100%;
        align-items: stretch;
        gap: 10px;
    }

    .review-form {
      display: flex;
      flex-direction: column;
      gap: 20px;
      padding: 10px 0;
    }

    .rating-block label, .comment-block label {
      font-size: 16px;
      font-weight: 600;
      color: #333;
      margin-bottom: 10px;
      display: block;
    }

    .stars {
      display: flex;
      gap: 5px;
      font-size: 40px;
      color: #e5e7eb;
      cursor: pointer;
    }

    .star {
      transition: color 0.2s;
    }

    .star:hover, .star.active {
      color: #F3C301;
    }

    .comment-block textarea {
      width: 100%;
      padding: 15px;
      border-radius: 12px;
      border: 1px solid #ddd;
      font-family: var(--main-font);
      font-size: 15px;
      resize: vertical;
      outline: none;
    }

    .comment-block textarea:focus {
      border-color: var(--primary-color);
    }

    .modal-actions {
      display: flex;
      justify-content: flex-end;
      gap: 15px;
      margin-top: 10px;
    }

    .modal-actions button {
      padding: 10px 20px;
      border-radius: 10px;
      font-weight: 600;
      font-size: 16px;
      cursor: pointer;
      font-family: var(--main-font);
    }

    .cancel-btn {
      background: transparent;
      border: 1px solid #ccc;
      color: #666;
    }

    .submit-btn {
      background: var(--primary-color);
      color: white;
      border: none;
    }

    .submit-btn:disabled {
      background: #ccc;
      cursor: not-allowed;
    }
</style>