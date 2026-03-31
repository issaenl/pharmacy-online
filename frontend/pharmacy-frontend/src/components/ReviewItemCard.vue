<template>
  <div class="review-card" :class="{ 'is-rejected': review.status === 2 }">
    <div class="review-header">
      <div class="header-left">
        <div class="pharmacy-name">Аптека: {{ review.pharmacyName }}</div>
        <div class="review-date">{{ formatDate(review.createdAt) }} (Заказ №{{ review.orderId }})</div>
      </div>
      
      <div class="header-right">
        <div class="review-status" :class="statusClass">{{ statusText }}</div>
      </div>
    </div>

    <div class="review-body">
      <div class="rating-stars">
        <span 
          v-for="n in 5" 
          :key="n" 
          class="star" 
          :class="{ 'filled': n <= review.rating }"
        >
          ★
        </span>
      </div>

      <div class="comment-block">
        <p v-if="review.comment" class="comment-text">«{{ review.comment }}»</p>
        <p v-else class="comment-empty">Без текста</p>
      </div>
    </div>

    <div v-if="review.status === 2 && review.rejectReason" class="reject-reason-block">
      <div class="reject-title">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>
        Причина отклонения:
      </div>
      <p class="reject-text">{{ review.rejectReason }}</p>
      <div class="reject-hint">Вы можете оставить новый отзыв для этого заказа в разделе «История заказов».</div>
    </div>

  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  review: {
    type: Object,
    required: true
  }
});

const formatDate = (dateString) => {
  const options = { day: '2-digit', month: 'long', year: 'numeric' };
  return new Date(dateString).toLocaleDateString('ru-RU', options);
};

const statusText = computed(() => {
  const statuses = {
    0: 'На модерации',
    1: 'Опубликован',
    2: 'Отклонен'
  };
  return statuses[props.review.status] || 'Неизвестно';
});

const statusClass = computed(() => {
  const classes = {
    0: 'status-pending',
    1: 'status-approved',
    2: 'status-rejected'
  };
  return classes[props.review.status] || '';
});
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

.review-card:hover {
  box-shadow: 0 4px 15px rgba(0,0,0,0.08);
}

.review-card.is-rejected {
  border-color: #FDE8E8;
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
  padding-bottom: 15px;
  border-bottom: 1px dashed #eee;
}

.pharmacy-name {
  font-weight: 700;
  font-size: 18px;
  color: #333;
  margin-bottom: 4px;
}

.review-date {
  font-size: 14px;
  color: #888;
}

.review-status {
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
}

.status-pending { background: #fcf1d6; color: #d97706; }
.status-approved { background: #E8F4EA; color: var(--primary-color, #689D6D); }
.status-rejected { background: #FDE8E8; color: #BB4E58; }

.review-body {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.rating-stars {
  font-size: 24px;
  color: #e5e7eb;
  letter-spacing: 2px;
}

.star.filled {
  color: #F3C301;
}

.comment-block {
  margin-top: 5px;
}

.comment-text {
  font-size: 16px;
  color: #333;
  margin: 0;
  line-height: 1.5;
  font-style: italic;
}

.comment-empty {
  font-size: 15px;
  color: #aaa;
  margin: 0;
  font-style: italic;
}

.reject-reason-block {
  margin-top: 20px;
  background: #fff5f5;
  border-radius: 12px;
  padding: 15px;
  border: 1px solid #FDE8E8;
}

.reject-title {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #BB4E58;
  font-weight: 700;
  font-size: 15px;
  margin-bottom: 5px;
}

.reject-text {
  color: #333;
  font-size: 15px;
  margin: 0 0 10px 0;
  line-height: 1.4;
}

.reject-hint {
  font-size: 13px;
  color: #888;
}

@media (max-width: 600px) {
  .review-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
}
</style>