<template>
  <div class="review-card">
    <div class="review-header">
      <div class="header-left">
        <div class="pharmacy-name">{{ review.pharmacyName }} <i style="color: #689D6D;">(Заказ №{{ review.orderId }})</i></div>
        <div class="review-date">{{ formatDate(review.createdAt) }}</div>
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
.status-approved { background: #E8F4EA; color: #689D6D; }
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

@media (max-width: 600px) {
  .review-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
}
</style>