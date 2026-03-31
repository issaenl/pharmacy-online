<template>
  <TheHeader />
  <div v-if="loading" class="container loading-text">Загрузка данных аптеки...</div>
  
  <div v-else-if="pharmacy" class="pharmacy-page container">
    <div class="page-header">
      <div class="title-row">
        <h1 class="pharmacy-title">{{ pharmacy.name }}</h1>
      </div>
      <p class="last-update" v-if="assortment.length > 0">
        Последнее обновление: {{ latestUpdateFormatted }}
      </p>
    </div>

    <section class="pharmacy-info-card">
      <div class="info-grid">
        <div class="info-column">
          <div class="info-block">
            <span class="info-label">Адрес</span>
            <p class="info-text">{{ pharmacy.district }}, {{ pharmacy.address }}</p>
            <a :href="`https://yandex.ru/maps/?text=${pharmacy.latitude},${pharmacy.longitude}`" target="_blank" class="map-link">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path><circle cx="12" cy="10" r="3"></circle></svg>
              Показать на карте
            </a>
          </div>
          
          <div class="info-block">
            <span class="info-label">Телефон</span>
            <a :href="`tel:${cleanPhone(pharmacy.phone)}`" class="info-text phone-text phone-link">
              {{ pharmacy.phone }}
            </a>
          </div>
        </div>

       <div class="photo-column">
          <div 
            v-show="hasPanorama" 
            id="panorama-container" 
            class="pharmacy-photo panorama-block">
          </div>
          
          <img 
            v-show="!hasPanorama" 
            :src="pharmacy?.photoUrl || '/assets/pharmacy-placeholder.jpg'" 
            alt="Фото аптеки" 
            class="pharmacy-photo"
          >
        </div>
      </div>
    </section>

    <section class="pharmacy-reviews-card">
      <div class="reviews-card-header">
        <h2>Отзывы покупателей</h2>
      </div>

      <div class="reviews-stats-container">
        <div class="rating-bars">
          <div class="rating-bar-row" v-for="stat in ratingStats" :key="stat.stars">
            <span class="star-label">{{ stat.stars }} <span class="star-icon">★</span></span>
            <div class="progress-track">
              <div class="progress-fill" :style="{ width: stat.percent + '%' }"></div>
            </div>
            <span class="count-label">{{ stat.count }}</span>
          </div>
        </div>

        <div class="overall-rating-box">
          <div class="rating-number">{{ pharmacy.rating ? pharmacy.rating.toFixed(1) : '0.0' }}</div>
          <div class="rating-stars-big">
            <span v-for="n in 5" :key="n" :class="{'filled': n <= Math.round(pharmacy.rating || 0)}">★</span>
          </div>
          <div class="rating-count">Оценок: {{ pharmacyReviews.length }}</div>
        </div>
      </div>

      <div class="reviews-list-container" v-if="pharmacyReviews.length > 0">
        <div class="reviews-grid">
          <div v-for="rev in pharmacyReviews.slice(0, 3)" :key="rev.id" class="review-bubble">
            <div class="bubble-header">
              <div class="bubble-user">
                <div class="avatar">{{ rev.userName.charAt(0) }}</div>
                <div class="user-meta">
                  <div class="author-name">{{ rev.userName }}</div>
                  <div class="bubble-stars">
                    <span v-for="n in 5" :key="n" :class="{'filled': n <= rev.rating}">★</span>
                  </div>
                </div>
              </div>
              <div class="review-date">{{ formatDate(rev.createdAt) }}</div>
            </div>
            <p class="bubble-text" v-if="rev.comment">{{ rev.comment }}</p>
          </div>
        </div>

        <div class="reviews-footer">
          <router-link :to="`/pharmacy/${pharmacy.id}/reviews`" class="all-reviews-btn">
            Смотреть все отзывы
          </router-link>
        </div>
      </div>
      
      <div v-else class="empty-reviews-state">
        Здесь пока нет отзывов. Вы можете стать первым!
      </div>
    </section>

    <section class="assortment-section">
      <div class="assortment-header">
        <h2>Ассортимент аптеки</h2>
        <div class="search-box">
          <svg class="search-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
          <input type="text" v-model="searchQuery" placeholder="Поиск лекарств в этой аптеке..." class="search-input">
        </div>
      </div>

      <div v-if="filteredAssortment.length === 0" class="empty-assortment">
        Товары не найдены
      </div>
      
      <div v-else class="assortment-grid">
        <ProductCard 
          v-for="item in displayedAssortment" 
          :key="item.productId" 
          :product="{
            id: item.productId,
            name: item.productName,
            dosageForm: item.dosageForm,
            pictureUrl: item.pictureUrl,
            minPrice: item.price,
            expirationDate: item.expirationDate  }"
          :exact-price="true"  />
      </div>

      <AppPagination 
        :current-page="currentPage" 
        :total-pages="totalPages" 
        @update:currentPage="setPage" 
      />
    </section>

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, nextTick } from 'vue';
import { useRoute } from 'vue-router';
import { useToast } from 'vue-toast-notification';
import ProductCard from '@/components/ProductCard.vue'; 
import TheHeader from '@/components/Header.vue';
import AppPagination from '@/components/admin/AppPagination.vue';
import api from '@/api/api';

const route = useRoute();
const toast = useToast({ position: 'bottom-right' });

const pharmacy = ref(null);
const assortment = ref([]);
const pharmacyReviews = ref([]); 
const loading = ref(true);
const searchQuery = ref('');
const hasPanorama = ref(false);

const cleanPhone = (phone) => {
  return phone ? phone.replace(/[^\d+]/g, '') : '';
};

const formatDate = (dateString) => {
  const options = { day: '2-digit', month: 'short', year: 'numeric' };
  return new Date(dateString).toLocaleDateString('ru-RU', options);
};

const ratingStats = computed(() => {
  const stats = { 5: 0, 4: 0, 3: 0, 2: 0, 1: 0 };
  
  pharmacyReviews.value.forEach(r => {
    if (stats[r.rating] !== undefined) {
      stats[r.rating]++;
    }
  });

  const total = pharmacyReviews.value.length || 1;
  
  return [5, 4, 3, 2, 1].map(stars => ({
    stars,
    count: stats[stars],
    percent: Math.round((stats[stars] / total) * 100)
  }));
});

const latestUpdateFormatted = computed(() => {
  if (!assortment.value || assortment.value.length === 0) return '';
  const latestDate = assortment.value.reduce((latest, item) => {
    if (!item.lastUpdate) return latest;
    const d = new Date(item.lastUpdate);
    return d > latest ? d : latest;
  }, new Date(0));

  if (latestDate.getTime() === 0) return '';
  const time = latestDate.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
  const date = latestDate.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' });
  return `${date} в ${time}`;
});

const filteredAssortment = computed(() => {
  if (!searchQuery.value) return assortment.value;
  const q = searchQuery.value.toLowerCase();
  return assortment.value.filter(item => 
    item.productName.toLowerCase().includes(q) || 
    item.manufacturer.toLowerCase().includes(q)
  );
});

const currentPage = ref(1);
const itemsPerPage = 12; 

const totalPages = computed(() => Math.ceil(filteredAssortment.value.length / itemsPerPage));

const displayedAssortment = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage;
  return filteredAssortment.value.slice(start, start + itemsPerPage);
});

const setPage = (page) => {
  currentPage.value = page;
  const assortmentSection = document.querySelector('.assortment-section');
  if (assortmentSection) {
    window.scrollTo({ top: assortmentSection.offsetTop - 20, behavior: 'smooth' });
  }
};

watch(searchQuery, () => { currentPage.value = 1; });

const loadYandexMapScript = () => {
  return new Promise((resolve, reject) => {
    if (window.ymaps) { resolve(window.ymaps); return; }
    const apiKey = import.meta.env.VITE_YANDEX_MAP_API_KEY; 
    const script = document.createElement('script');
    script.src = apiKey ? `https://api-maps.yandex.ru/2.1/?apikey=${apiKey}&lang=ru_RU` : `https://api-maps.yandex.ru/2.1/?lang=ru_RU`;
    script.type = "text/javascript";
    script.onload = () => resolve(window.ymaps);
    script.onerror = reject;
    document.head.appendChild(script);
  });
};

const initPanorama = () => {
  if (!pharmacy.value?.latitude || !pharmacy.value?.longitude) return;
  window.ymaps.panorama.locate([pharmacy.value.latitude, pharmacy.value.longitude]).then(
    function (panoramas) {
      if (panoramas.length > 0) {
        hasPanorama.value = true;
        nextTick(() => {
          new window.ymaps.panorama.Player('panorama-container', panoramas[0], { direction: 'auto' });
        });
      } else { hasPanorama.value = false; }
    },
    function (error) { console.error("Ошибка загрузки панорамы:", error); hasPanorama.value = false; }
  );
};

onMounted(async () => {
  const id = route.params.id;
  try {
    const [pharmRes, assortRes, reviewsRes] = await Promise.all([
      api.get(`/Pharmacies/${id}`),
      api.get(`/Pharmacies/${id}/assortiment`),
      api.get(`/Reviews/pharmacy/${id}`).catch(() => ({ data: [] })) 
    ]);
    pharmacy.value = pharmRes.data;
    assortment.value = assortRes.data;
    pharmacyReviews.value = reviewsRes.data;

    try {
      const ymaps = await loadYandexMapScript();
      ymaps.ready(initPanorama);
    } catch (e) {
      console.error("Не удалось инициализировать карты", e);
    }
  } catch (error) {
    toast.error("Ошибка загрузки данных аптеки");
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.pharmacy-page {
  padding: 40px 0;
  font-family: var(--main-font);
}

.loading-text { text-align: center; padding: 100px; font-size: 20px; color: #888; }
.page-header { margin-bottom: 30px; }

.title-row { display: flex; align-items: center; gap: 15px; margin-bottom: 5px; }
.pharmacy-title { font-size: 32px; font-weight: 700; color: #000; margin: 0; }
.last-update { font-size: 14px; color: #888; margin: 0; }

.pharmacy-info-card {
  background: #fff;
  border-radius: 30px;
  padding: 40px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.03);
  margin-bottom: 30px;
}

.info-grid { display: flex; gap: 40px; }
.info-column { flex: 1; display: flex; flex-direction: column; gap: 20px; }
.info-block { display: flex; flex-direction: column; gap: 8px; }
.info-label { font-size: 14px; color: #888; text-transform: uppercase; letter-spacing: 0.5px; }
.info-text { font-size: 18px; color: #333; margin: 0; line-height: 1.4; }

.phone-link {
  text-decoration: none; color: var(--primary-color, #689D6D); font-weight: 600; transition: opacity 0.2s;
}
.phone-link:hover { text-decoration: underline; opacity: 0.8; }

.map-link {
  display: inline-flex; align-items: center; gap: 5px; color: var(--primary-color, #689D6D); text-decoration: none; font-size: 16px; font-weight: 500; margin-top: 5px;
}
.map-link:hover { text-decoration: underline; }

.photo-column { flex: 1; display: flex; justify-content: flex-end; }
.pharmacy-photo { width: 100%; max-width: 500px; height: 300px; object-fit: cover; border-radius: 20px; overflow: hidden; }
.panorama-block { background: #eee; }

.pharmacy-reviews-card {
  background: #fff;
  border-radius: 30px;
  padding: 40px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.03);
  margin-bottom: 40px;
}

.reviews-card-header h2 {
  font-size: 24px;
  margin: 0 0 25px 0;
  color: #000;
}

.reviews-stats-container {
  display: flex;
  align-items: center;
  gap: 50px;
  margin-bottom: 40px;
}

.rating-bars {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.rating-bar-row {
  display: flex;
  align-items: center;
  gap: 15px;
}

.star-label {
  font-size: 15px;
  font-weight: 600;
  color: #333;
  width: 40px;
  display: flex;
  justify-content: space-between;
}

.star-icon {
  color: var(--primary-color, #689D6D);
}

.progress-track {
  flex: 1;
  height: 8px;
  background: #E8F4EA;
  border-radius: 10px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--primary-color, #689D6D);
  border-radius: 10px;
  transition: width 0.5s ease-out;
}

.count-label {
  width: 30px;
  font-size: 14px;
  color: #555;
  text-align: right;
}

.overall-rating-box {
  background: #F8FBF8;
  padding: 30px 40px;
  border-radius: 20px;
  text-align: center;
  min-width: 200px;
  border: 1px solid #E8F4EA;
}

.rating-number {
  font-size: 48px;
  font-weight: 700;
  color: var(--primary-color, #689D6D);
  line-height: 1;
}

.rating-stars-big {
  color: #e5e7eb;
  font-size: 24px;
  letter-spacing: 3px;
  margin: 10px 0;
}
.rating-stars-big .filled { color: var(--primary-color, #689D6D); }

.rating-count {
  font-size: 14px;
  color: #555;
  font-weight: 500;
}

.reviews-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  margin-bottom: 25px;
}

.review-bubble {
  background: #FAFAFA;
  border-radius: 20px;
  border-bottom-left-radius: 4px;
  padding: 20px;
  border: 1px solid #E8F4EA;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.bubble-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.bubble-user {
  display: flex;
  align-items: center;
  gap: 10px;
}

.avatar {
  width: 40px; 
  height: 40px;
  border-radius: 50%;
  background: #E8F4EA;
  color: var(--primary-color, #689D6D);
  display: flex; align-items: center; justify-content: center;
  font-weight: bold;
  font-size: 18px;
}

.user-meta { display: flex; flex-direction: column; }
.author-name { font-weight: 600; font-size: 15px; color: #333; }
.bubble-stars { color: #e5e7eb; font-size: 14px; letter-spacing: 1px; line-height: 1; margin-top: 2px;}
.bubble-stars .filled { color: var(--primary-color, #689D6D); } 

.review-date { font-size: 13px; color: #aaa; }
.bubble-text { font-size: 15px; color: #444; line-height: 1.5; margin: 0; font-style: italic; }

.empty-reviews-state {
  text-align: center;
  color: #888;
  padding: 20px;
  background: #f9f9f9;
  border-radius: 16px;
}

.reviews-footer { text-align: center; }

.all-reviews-btn {
  display: inline-block;
  padding: 12px 30px;
  background: transparent;
  color: var(--primary-color, #689D6D);
  border: 1.5px solid var(--primary-color, #689D6D);
  border-radius: 12px;
  font-weight: 600;
  text-decoration: none;
  transition: 0.2s;
}

.all-reviews-btn:hover {
  background: var(--primary-color, #689D6D);
  color: white;
}

.assortment-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.assortment-header h2 { font-size: 24px; margin: 0; }
.search-box { display: flex; align-items: center; background: #fff; border-radius: 15px; padding: 5px 15px; width: 400px; border: 1px solid #eee; }
.search-icon { color: #888; margin-right: 10px; }
.search-input { border: none; outline: none; width: 100%; font-size: 16px; font-family: inherit; }

.assortment-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 20px; margin-bottom: 20px; }
.empty-assortment { text-align: center; padding: 40px; color: #888; background: #fff; border-radius: 16px; }


@media (max-width: 1100px) { 
  .assortment-grid { grid-template-columns: repeat(3, 1fr); } 
  .reviews-grid { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 900px) {
  .info-grid { flex-direction: column; }
  .pharmacy-photo { height: 250px; max-width: 100%; }
  .assortment-header { flex-direction: column; align-items: flex-start; gap: 15px; }
  .search-box { width: 100%;}
  .reviews-stats-container { flex-direction: column; gap: 30px; align-items: stretch; }
}

@media (max-width: 768px) {
  .pharmacy-page { padding: 30px 15px; }
  .pharmacy-info-card, .pharmacy-reviews-card { padding: 20px; }
  .assortment-grid { grid-template-columns: repeat(2, 1fr); }
  .reviews-grid { grid-template-columns: 1fr; }
}

@media (max-width: 480px) {
  .title-row { flex-direction: column; align-items: flex-start; gap: 5px; }
  .assortment-grid { grid-template-columns: 1fr; }
  .pharmacy-title { font-size: 24px; }
  .pharmacy-photo { height: 200px; }
}

.panorama-block :deep(*) { box-sizing: content-box !important; }
.panorama-block :deep(button) { padding: 0 !important; margin: 0 !important; border: none; line-height: normal !important; }
</style>