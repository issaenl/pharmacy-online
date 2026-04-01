<template>
  <TheHeader />
  <div class="all-reviews-page">
    <div class="container">
      
      <div class="page-header" v-if="pharmacy">
        <router-link :to="`/pharmacy/${pharmacy.id}`" class="back-link">
          ← Вернуться к аптеке
        </router-link>
        <h1>Отзывы об аптеке «{{ pharmacy.name }}»</h1>
      </div>

      <div v-if="loading" class="loader">Загрузка отзывов...</div>

      <div v-else-if="reviews.length === 0" class="empty-state">
        У этой аптеки пока нет отзывов.
      </div>

      <div v-else class="reviews-layout">
        
        <div class="filters-horizontal">
          <button 
            :class="['filter-btn', { active: selectedFilter === null }]" 
            @click="selectedFilter = null"
          >
            Все ({{ reviews.length }})
          </button>

          <button 
            v-for="stat in ratingStats" 
            :key="stat.stars"
            :class="['filter-btn', { active: selectedFilter === stat.stars }]"
            @click="selectedFilter = stat.stars"
            :disabled="stat.count === 0"
          >
            {{ stat.stars }} <span class="star-icon">★</span> 
            <span class="count-text">({{ stat.count }})</span>
          </button>
        </div>

        <main class="reviews-content">
          <div v-if="filteredReviews.length === 0" class="empty-state-small">
            Нет отзывов с такой оценкой.
          </div>
          
          <div v-else class="reviews-list">
            <ReviewCard 
              v-for="review in paginatedReviews" 
              :key="review.id" 
              :review="review" 
            />
          </div>

         <AppPagination 
            v-if="totalPages > 1"
            :current-page="currentPage" 
            :total-pages="totalPages" 
            @update:currentPage="setPage" 
          />
        </main>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useToast } from 'vue-toast-notification';
import TheHeader from '@/components/Header.vue';
import ReviewCard from '@/components/ReviewCard.vue';
import AppPagination from '@/components/AppPagination.vue';
import api from '@/api/api';

const route = useRoute();
const toast = useToast({ position: 'bottom-right' });

const pharmacy = ref(null);
const reviews = ref([]);
const loading = ref(true);

const selectedFilter = ref(null);

const ratingStats = computed(() => {
  const stats = { 5: 0, 4: 0, 3: 0, 2: 0, 1: 0 };
  reviews.value.forEach(r => { if (stats[r.rating] !== undefined) stats[r.rating]++; });
  
  return [5, 4, 3, 2, 1].map(stars => ({
    stars,
    count: stats[stars]
  }));
});

const filteredReviews = computed(() => {
  if (selectedFilter.value === null) return reviews.value;
  return reviews.value.filter(r => r.rating === selectedFilter.value);
});

const currentPage = ref(1);
const itemsPerPage = 30;

const totalPages = computed(() => Math.ceil(filteredReviews.value.length / itemsPerPage));

const paginatedReviews = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage;
  return filteredReviews.value.slice(start, start + itemsPerPage);
});

const setPage = (page) => {
  currentPage.value = page;
  window.scrollTo({ top: 0, behavior: 'smooth' });
};

watch(selectedFilter, () => { currentPage.value = 1; });

onMounted(async () => {
  const id = route.params.id;
  try {
    const [pharmRes, reviewsRes] = await Promise.all([
      api.get(`/Pharmacies/${id}`),
      api.get(`/Reviews/pharmacy/${id}`)
    ]);
    pharmacy.value = pharmRes.data;
    reviews.value = reviewsRes.data;
  } catch (error) {
    toast.error("Ошибка загрузки отзывов");
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.all-reviews-page {
  padding: 40px 0;
  background: #F5F5F5;
  min-height: 80vh;
}

.container {
  max-width: 800px;
  margin: 0 auto;
  padding: 0 20px;
}

.page-header {
  margin-bottom: 30px;
}

.back-link {
  color: #888;
  text-decoration: none;
  font-size: 16px;
  display: inline-block;
  margin-bottom: 15px;
  transition: 0.2s;
}

.back-link:hover {
  color: var(--primary-color, #689D6D);
}

.page-header h1 {
  font-size: 32px;
  margin: 0;
  color: #000;
}

.loader, .empty-state, .empty-state-small {
  text-align: center;
  padding: 50px;
  color: #888;
  font-size: 18px;
  background: white;
  border-radius: 20px;
}

.reviews-layout {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

.filters-horizontal {
  display: flex;
  gap: 12px;
  overflow-x: auto;
  padding-bottom: 10px;
  scrollbar-width: none;
}

.filters-horizontal::-webkit-scrollbar {
  display: none;
}

.filter-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 10px 20px;
  background: white;
  border: 2px solid #E5E7EB;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 500;
  color: #555;
  cursor: pointer;
  white-space: nowrap;
  transition: all 0.2s ease;
  font-family: inherit;
}

.filter-btn:hover:not(:disabled) {
  border-color: #ccc;
  background: #f9f9f9;
}

.filter-btn.active {
  border-color: var(--primary-light);
  color: #000;
  font-weight: 700;
}

.filter-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
  background: #f9f9f9;
}

.star-icon {
  color: #689D6D;
  font-size: 16px;
}

.count-text {
  color: #888;
  font-size: 14px;
  font-weight: normal;
}

.filter-btn.active .count-text {
  color: #555;
}


.reviews-content {
  width: 100%;
}

.reviews-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

@media (max-width: 768px) {
  .all-reviews-page {
    padding: 20px 0;
  }
  
  .page-header h1 {
    font-size: 24px;
  }
  
  .filter-btn {
    padding: 8px 16px;
    font-size: 15px;
  }
}
</style>