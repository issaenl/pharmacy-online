<template>
  <div class="profile-reviews-component">
    <div class="section-header">
      <h2>Мои отзывы</h2>
    </div>
    
    <div v-if="isLoading" class="loading-state">
      <p>Загрузка отзывов...</p>
    </div>

    <div v-else-if="reviews.length === 0" class="empty-state">
      <div class="empty-icon">⭐</div>
      <p>Вы еще не оставляли отзывы.</p>
      <span class="hint-text">Оценить аптеку можно в разделе "История заказов" после получения заказа.</span>
    </div>

    <div v-else class="reviews-list">
      <ReviewItemCard 
        v-for="review in reviews" 
        :key="review.id" 
        :review="review" 
      />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import api from '@/api/api';
import { useToast } from 'vue-toast-notification';
import ReviewItemCard from '@/components/ReviewItemCard.vue';

const reviews = ref([]);
const isLoading = ref(true);
const toast = useToast({ position: 'bottom-right' });

const fetchReviews = async () => {
  try {
    isLoading.value = true;
    const response = await api.get('/Reviews/my-reviews');
    reviews.value = response.data;
  } catch (error) {
    console.error('Ошибка загрузки отзывов:', error);
    toast.error('Не удалось загрузить отзывы');
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchReviews();
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
  font-size: 48px;
  opacity: 0.3;
  margin-bottom: 15px;
  filter: grayscale(100%);
}

.empty-state p {
  font-size: 18px;
  color: #333;
  font-weight: 600;
  margin: 0 0 10px 0;
}

.hint-text {
  font-size: 14px;
  color: #888;
}

.profile-reviews-component {
  width: 100%;
}

.reviews-list {
  display: flex;
  flex-direction: column;
  width: 100%;
  gap: 10px;
}
</style>