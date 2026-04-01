<template>
  <div class="review-card" :class="{ 'is-rejected': review.status === 2 }">
    
    <div class="review-header">
      <div class="header-left">
        <div class="pharmacy-name">Аптека: {{ review.pharmacyName }}</div>
        <div class="review-date">{{ formatDate(review.createdAt) }} (Заказ №{{ review.orderId }})</div>
      </div>
      
      <div class="header-right">
        <div class="review-status" :class="statusClass">{{ statusText }}</div>
        
        <div class="card-actions" v-if="!isEditing">
          <button class="btn-action edit-action" @click="startEditing" title="Редактировать">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
              <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
            </svg>
          </button>
          
          <button class="btn-action delete-action" @click="deleteReview" title="Удалить">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6"></polyline>
              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
            </svg>
          </button>
        </div>
      </div>
    </div>

    <div class="review-body" v-if="!isEditing">
      <div class="rating-stars">
        <span v-for="n in 5" :key="n" class="star" :class="{ 'filled': n <= review.rating }">★</span>
      </div>

      <div class="comment-block">
        <p v-if="review.comment" class="comment-text">«{{ review.comment }}»</p>
        <p v-else class="comment-empty">Без текста</p>
      </div>
    </div>

    <form class="review-edit-form" v-else @submit.prevent="submitEdit">
      <label class="edit-label">Оценка:</label>
      <div class="rating-stars-edit">
        <span 
          v-for="n in 5" 
          :key="n" 
          class="star-edit" 
          :class="{ 'filled': n <= editForm.rating }"
          @click="editForm.rating = n"
        >
          ★
        </span>
      </div>

      <label class="edit-label">Текст отзыва:</label>
      <textarea 
        v-model="editForm.comment" 
        rows="3" 
        class="edit-textarea" 
        placeholder="Поделитесь своими впечатлениями..."
      ></textarea>

      <div class="edit-actions">
        <button type="submit" class="btn-save" :disabled="isLoading">
          {{ isLoading ? 'Сохранение...' : 'Отправить на проверку' }}
        </button>
        <button type="button" @click="isEditing = false" class="btn-cancel">Отмена</button>
      </div>
    </form>

    <div v-if="!isEditing && review.status === 2 && review.rejectReason" class="reject-reason-block">
      <div class="reject-title">Причина отклонения:</div>
      <p class="reject-text">{{ review.rejectReason }}</p>
      <div class="reject-hint">Нажмите кнопку редактирования выше, чтобы исправить отзыв.</div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useToast } from 'vue-toast-notification';
import api from '@/api/api';

const props = defineProps({
  review: {
    type: Object,
    required: true
  }
});

const emit = defineEmits(['review-updated', 'review-deleted']);
const toast = useToast({ position: 'bottom-right' });

const isEditing = ref(false);
const isLoading = ref(false);

const editForm = ref({
  rating: 5,
  comment: ''
});

const formatDate = (dateString) => {
  const options = { day: '2-digit', month: 'long', year: 'numeric' };
  return new Date(dateString).toLocaleDateString('ru-RU', options);
};

const statusText = computed(() => {
  const statuses = { 0: 'На модерации', 1: 'Опубликован', 2: 'Отклонен' };
  return statuses[props.review.status] || 'Неизвестно';
});

const statusClass = computed(() => {
  const classes = { 0: 'status-pending', 1: 'status-approved', 2: 'status-rejected' };
  return classes[props.review.status] || '';
});

const startEditing = () => {
  editForm.value = {
    rating: props.review.rating,
    comment: props.review.comment || ''
  };
  isEditing.value = true;
};

const submitEdit = async () => {
  isLoading.value = true;
  try {
    const response = await api.put(`/Reviews/${props.review.id}`, editForm.value);
    toast.success(response.data.message || "Отзыв отправлен на проверку");
    isEditing.value = false;
    emit('review-updated'); 
  } catch (error) {
    toast.error(error.response?.data || "Ошибка при сохранении отзыва");
  } finally {
    isLoading.value = false;
  }
};

const deleteReview = async () => {
  if (!confirm("Вы уверены, что хотите навсегда удалить этот отзыв?")) return;
  
  try {
    await api.delete(`/Reviews/${props.review.id}`);
    toast.success("Отзыв удален");
    emit('review-deleted'); 
  } catch (error) {
    toast.error("Ошибка при удалении отзыва");
  }
};
</script>

<style scoped>
.review-card {
  background: white;
  border-radius: 16px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
  margin-bottom: 15px;
  font-family: var(--main-font);
  transition: 0.3s;
  border: 1px solid transparent;
}

.review-card:hover { box-shadow: 0 4px 15px rgba(0,0,0,0.08); }
.review-card.is-rejected { border-color: #FDE8E8; }

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
  padding-bottom: 15px;
  border-bottom: 1px dashed #eee;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 15px;
}

.pharmacy-name { font-weight: 700; font-size: 18px; color: #333; margin-bottom: 4px; }
.review-date { font-size: 14px; color: #888; }

.review-status { padding: 6px 12px; border-radius: 8px; font-size: 14px; font-weight: 600; }
.status-pending { background: #fcf1d6; color: #d97706; }
.status-approved { background: #E8F4EA; color: var(--primary-color, #689D6D); }
.status-rejected { background: #FDE8E8; color: #BB4E58; }

.card-actions { 
  display: flex; 
  gap: 6px; 
}

.btn-action {
  background: none;
  border: none;
  padding: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #A0A0A0; 
  transition: all 0.2s ease;
  border-radius: 8px;
}

.btn-action:hover {
  transform: scale(1.05); 
  background: #f5f5f5;
}

.edit-action:hover { 
  color: var(--primary-color, #689D6D); 
  background: #E8F4EA;
} 

.delete-action:hover { 
  color: #BB4E58; 
  background: #FDE8E8;
}

.review-body { display: flex; flex-direction: column; gap: 10px; }
.rating-stars { font-size: 24px; color: #e5e7eb; letter-spacing: 2px; }
.star.filled { color: #F3C301; }
.comment-text { font-size: 16px; color: #333; margin: 0; line-height: 1.5; font-style: italic; }
.comment-empty { font-size: 15px; color: #aaa; margin: 0; font-style: italic; }

.reject-reason-block { margin-top: 20px; background: #fff5f5; border-radius: 12px; padding: 15px; border: 1px solid #FDE8E8; }
.reject-title { color: #BB4E58; font-weight: 700; font-size: 15px; margin-bottom: 5px; }
.reject-text { color: #333; font-size: 15px; margin: 0 0 10px 0; line-height: 1.4; }
.reject-hint { font-size: 13px; color: #888; }

.review-edit-form {
  background: #F9F9F9;
  padding: 15px;
  border-radius: 12px;
  border: 1px solid #eee;
}

.edit-label { display: block; font-weight: 600; margin-bottom: 8px; color: #555; font-size: 14px;}

.rating-stars-edit {
  font-size: 32px; color: #e5e7eb; letter-spacing: 5px; margin-bottom: 15px; cursor: pointer;
}
.star-edit { transition: 0.2s; }
.star-edit:hover { transform: scale(1.1); }
.star-edit.filled { color: #F3C301; }

.edit-textarea {
  width: 100%; padding: 12px; border: 1px solid #ddd; border-radius: 10px;
  font-family: inherit; font-size: 15px; resize: vertical; margin-bottom: 15px;
  box-sizing: border-box; outline: none; transition: 0.2s;
}
.edit-textarea:focus { border-color: var(--primary-color, #689D6D); }

.edit-actions { display: flex; gap: 10px; }
.btn-save {
  background: var(--primary-color, #689D6D); 
  color: white; 
  border: none;
  padding: 10px 20px; 
  border-radius: 8px; 
  font-weight: 600; 
  font-family: var(--main-font);
  cursor: pointer; 
  transition: 0.2s;
}
.btn-save:hover:not(:disabled) { opacity: 0.9; }
.btn-save:disabled { opacity: 0.6; cursor: not-allowed; }

.btn-cancel {
  background: #e0e0e0; 
  color: #555; 
  border: none; 
  padding: 10px 20px;
  border-radius: 8px; 
  font-weight: 600; 
  font-family: var(--main-font);
  cursor: pointer; 
  transition: 0.2s;
}
.btn-cancel:hover { background: #ccc; }

@media (max-width: 600px) {
  .review-header { flex-direction: column; align-items: flex-start; gap: 10px; }
  .header-right { width: 100%; justify-content: space-between; }
}
</style>