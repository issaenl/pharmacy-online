<template>
  <TheHeader />
  <div v-if="loading" class="container">Загрузка...</div>
  
  <div v-else-if="product" class="product-page container">
    <nav class="breadcrumbs">
      Каталог > {{ product.categoryName || 'Лекарства' }} > {{ product.name }}
    </nav>
    
    <section class="product-main card">
      <div class="product-visual">
        <button 
          class="wishlist-btn" 
          :class="{ 'is-favorite': product && favoriteStore.isFavorite(product.id) }" 
          @click="toggleFavorite"
          title="Добавить в избранное"
        >
          <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
          </svg>
        </button>
        
        <img :src="product.pictureUrl || '/assets/no-image.jpg'" :alt="product.name">
      </div>

      <div class="product-details">
        <div class="info-group">
          <h1>{{ product.name }}, {{ product.dosageForm }}</h1>
          <p class="manufacturer">{{ product.manufacturer }}, {{ product.country }}</p>
          <p :class="['prescription-status', { 'is-red': product.isPrescription }]">
            {{ product.isPrescription ? 'Рецептурный препарат' : 'Безрецептурный препарат' }}
          </p>
        </div>

        <div class="purchase-group">
          <div class="price-range">{{ formatPrice(product.minPrice) }} — {{ formatPrice(product.maxPrice) }} р.</div>
          <p class="disclaimer">Внешний вид товара может отличаться от изображенного на фотографии</p>
        </div>
      </div>

      <div class="action-column">
        <button 
          v-if="product.pdfUrl" 
          class="btn-outline instruction-btn" 
          @click="openInstruction"
        >
          Инструкция <span class="arrow">></span>
        </button>
        
        <button v-else class="btn-outline instruction-btn" disabled>
          Инструкция <span class="arrow">></span>
        </button>
        
        <button class="btn-primary cart-btn" @click="addToCart">
          <img src="/assets/Cart.svg" alt="" class="cart-icon-white">
          В корзину
        </button>
      </div>
    </section>

    <section class="pharmacies-section">
      <div class="pharmacies-header">
        <h2>Наличие в аптеках</h2>
        
        <div class="view-tabs">
          <button :class="{ active: viewMode === 'list' }" @click="viewMode = 'list'">Списком</button>
          <button :class="{ active: viewMode === 'map' }" @click="viewMode = 'map'; loadMapIfNeeded()">На карте</button>
        </div>
      </div>

      <div class="filters-panel">
        <div class="filter-group">
          <label>Регион:</label>
          <select v-model="selectedDistrict" class="filter-select">
            <option value="">Все регионы</option>
            <option v-for="district in availableDistricts" :key="district" :value="district">
              {{ district }}
            </option>
          </select>
        </div>

        <div class="filter-group">
          <label>Сортировка:</label>
          <select v-model="sortBy" class="filter-select">
            <option value="price_asc">Сначала дешевые</option>
            <option value="price_desc">Сначала дорогие</option>
          </select>
        </div>
      </div>

      <div v-show="viewMode === 'list'" class="pharmacy-list">
        <div v-if="filteredPharmacies.length === 0" class="no-stock">Товар временно отсутствует по заданным фильтрам!</div>
        <PharmacyItem 
          v-for="shop in paginatedPharmacies" 
          :key="shop.id" 
          :pharmacy="shop"
          @book="handleBookClick"
        />
        
        <div v-if="filteredPharmacies.length > itemsPerPage" class="pagination">
          <button class="page-arrow" @click="setPage(currentPage - 1)" :disabled="currentPage === 1">‹</button>
          <button v-for="page in totalPages" :key="page" :class="['page-num', { active: currentPage === page }]" @click="setPage(page)">{{ page }}</button>
          <button class="page-arrow" @click="setPage(currentPage + 1)" :disabled="currentPage === totalPages">›</button>
        </div>
      </div>

      <div v-show="viewMode === 'map'" class="map-view-container">
        <div id="product-map" class="yandex-map-block">
          <div v-if="isMapLoading" class="map-loading">Загрузка карты...</div>
        </div>
      </div>
    </section>

    <div class="pdf-fullscreen-overlay" v-if="isPdfOpen">
      <div class="pdf-toolbar">
        <span class="pdf-title">Инструкция: {{ product.name }}</span>
        <button class="pdf-close-btn" @click="closeInstruction">Закрыть ✕</button>
      </div>
      <iframe :src="product.pdfUrl" class="pdf-iframe" title="Инструкция по применению">
        <p>Ваш браузер не поддерживает предпросмотр PDF. <a :href="product.pdfUrl" target="_blank">Скачать файл</a></p>
      </iframe>
    </div>
    
    <QuickOrderModal
      :is-open="isCheckoutModalOpen"
      :product="product"
      :pharmacy="selectedCheckoutPharmacy"
      :is-loading="isCheckoutLoading"
      @close="isCheckoutModalOpen = false"
      @confirm="submitQuickCheckout"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useCartStore } from '@/stores/cartStore';
import { useAuthStore } from '@/stores/authStore';
import { useFavoriteStore } from '@/stores/favoriteStore';
import { useToast } from 'vue-toast-notification';
import { useOrderStore } from '@/stores/orderStore';
import PharmacyItem from '@/components/PharmacyItem.vue';
import TheHeader from '@/components/Header.vue';
import QuickOrderModal from '@/components/QuickOrderModal.vue';
import api from '@/api/api';

const route = useRoute();
const router = useRouter();
const cartStore = useCartStore();
const authStore = useAuthStore();
const favoriteStore = useFavoriteStore();
const orderStore = useOrderStore();
const toast = useToast({ position: 'bottom-right' });

const product = ref(null);
const pharmacies = ref([]);
const loading = ref(true);

const selectedDistrict = ref('');
const sortBy = ref('price_asc');
const viewMode = ref('list'); 

const availableDistricts = computed(() => {
  const districts = pharmacies.value.map(p => p.pharmacyAddress?.split(',')[0]?.trim() || ''); // Простая заглушка. Лучше если бэкенд будет присылать поле District
  return [...new Set(districts)].filter(Boolean);
});

const filteredPharmacies = computed(() => {
  let result = [...pharmacies.value];

  if (selectedDistrict.value) {
    result = result.filter(p => p.pharmacyAddress.includes(selectedDistrict.value));
  }

  if (sortBy.value === 'price_asc') {
    result.sort((a, b) => a.price - b.price);
  } else if (sortBy.value === 'price_desc') {
    result.sort((a, b) => b.price - a.price);
  }

  return result;
});

const currentPage = ref(1);
const itemsPerPage = 10;

const paginatedPharmacies = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage;
  const end = start + itemsPerPage;
  return filteredPharmacies.value.slice(start, end);
});

const totalPages = computed(() => Math.ceil(filteredPharmacies.value.length / itemsPerPage));

const setPage = (page) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page;
    window.scrollTo({ top: 500, behavior: 'smooth' }); 
  }
}

watch([selectedDistrict, sortBy], () => {
  currentPage.value = 1;
  if (viewMode.value === 'map' && myMap) {
    renderMapObjects();
  }
});

const isMapLoading = ref(false);
let isMapScriptLoaded = false;
let myMap = null;

const loadMapIfNeeded = async () => {
  if (isMapScriptLoaded) return;
  
  isMapLoading.value = true;
  const apiKey = import.meta.env.VITE_YANDEX_MAP_API_KEY; 
  const url = apiKey ? `https://api-maps.yandex.ru/2.1/?apikey=${apiKey}&lang=ru_RU` : `https://api-maps.yandex.ru/2.1/?lang=ru_RU`;

  const script = document.createElement('script');
  script.src = url;
  script.type = "text/javascript";
  
  script.onload = () => {
    window.ymaps.ready(() => {
      isMapScriptLoaded = true;
      initMap();
      isMapLoading.value = false;
    });
  };
  document.head.appendChild(script);
};

const initMap = () => {
  const centerCoords = filteredPharmacies.value.length > 0 && filteredPharmacies.value[0].pharmacyLatitude
    ? [filteredPharmacies.value[0].pharmacyLatitude, filteredPharmacies.value[0].pharmacyLongitude] 
    : [53.9006, 27.5590]; 

  myMap = new window.ymaps.Map("product-map", {
    center: centerCoords,
    zoom: 11,
    controls: ['zoomControl', 'fullscreenControl']
  });

  renderMapObjects();
};

const renderMapObjects = () => {
  if (!myMap) return;
  myMap.geoObjects.removeAll();
  
  const clusterer = new window.ymaps.Clusterer({
      preset: 'islands#invertedRedClusterIcons',
      groupByCoordinates: false,
      clusterDisableClickZoom: false,
      clusterHideIconOnBalloonOpen: false,
      geoObjectHideIconOnBalloonOpen: false
  });

  const geoObjects = [];

  filteredPharmacies.value.forEach(pharmacy => {
    if (pharmacy.pharmacyLatitude && pharmacy.pharmacyLongitude) {
      
      const expDate = pharmacy.expirationDate 
        ? new Date(pharmacy.expirationDate).toLocaleDateString('ru-RU') 
        : 'н/д';

      const placemark = new window.ymaps.Placemark([pharmacy.pharmacyLatitude, pharmacy.pharmacyLongitude], {
        hintContent: `${pharmacy.pharmacyName} — ${pharmacy.price} р.`,
        balloonContentHeader: pharmacy.pharmacyName,
        balloonContentBody: `
          <p>${pharmacy.pharmacyAddress}</p>
          <p><strong>В наличии:</strong> ${pharmacy.quantity} шт.</p>
          <p><strong>Годен до:</strong> <span style="color:#BB4E58">${expDate}</span></p>
          <p><strong>Цена:</strong> ${formatPrice(pharmacy.price)} р.</p>
          <button onclick="document.dispatchEvent(new CustomEvent('book-from-map', {detail: ${pharmacy.pharmacyId}}))" style="background:#BB4E58; color:white; padding:8px 15px; border:none; border-radius:8px; cursor:pointer; width:100%; margin-top:10px; font-family: inherit;">Забронировать</button>
        `
      }, {
        preset: 'islands#redMedicalIcon' 
      });
      geoObjects.push(placemark);
    }
  });

  clusterer.add(geoObjects);
  myMap.geoObjects.add(clusterer);

  if (geoObjects.length > 0) {
    myMap.setBounds(clusterer.getBounds(), { checkZoomRange: true, zoomMargin: 20 });
  }
};

onMounted(() => {
  document.addEventListener('book-from-map', (e) => {
    const pId = e.detail;
    const selected = filteredPharmacies.value.find(p => p.pharmacyId === pId);
    if (selected) handleBookClick(selected);
  });
});
onUnmounted(() => {
  document.body.style.overflow = '';
  if (myMap) myMap.destroy();
});


const isPdfOpen = ref(false);
const isCheckoutModalOpen = ref(false);
const selectedCheckoutPharmacy = ref(null);
const isCheckoutLoading = ref(false);

const toggleFavorite = () => {
  if (product.value) favoriteStore.toggleFavorite(product.value);
};

const openInstruction = () => { isPdfOpen.value = true; document.body.style.overflow = 'hidden'; };
const closeInstruction = () => { isPdfOpen.value = false; document.body.style.overflow = ''; };

const formatPrice = (price) => price == null ? '0.00' : Number(price).toFixed(2);

const addToCart = () => {
  if (!product.value) return;
  cartStore.addToCart({ id: product.value.id, name: product.value.name, price: product.value.minPrice, imageUrl: product.value.pictureUrl });
};

const handleBookClick = (pharmacy) => {
  if (!authStore.token) { router.push('/login'); return; }
  selectedCheckoutPharmacy.value = pharmacy;
  isCheckoutModalOpen.value = true;
};

const submitQuickCheckout = async (quantity) => {
  if (!product.value || !selectedCheckoutPharmacy.value) return;
  isCheckoutLoading.value = true;
  const success = await orderStore.quickCheckout(product.value.id, selectedCheckoutPharmacy.value.pharmacyId, quantity);
  isCheckoutLoading.value = false;
  if (success) { isCheckoutModalOpen.value = false; router.push('/'); }
};

onMounted(async () => {
  const id = route.params.id;
  try {
    const productRes = await api.get(`/Products/${id}`);
    product.value = productRes.data;

    try {
      const stockRes = await api.get(`/Products/${id}/availibility`);
      pharmacies.value = stockRes.data;
    } catch (e) {
      pharmacies.value = []; 
    }
  } catch (error) {
    console.error("Ошибка загрузки товара:", error);
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
  .product-visual {
    position: relative; 
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0; 
  }

  .wishlist-btn {
    position: absolute; 
    top: 0px; 
    left: 0px; 
    background: none;
    border: none;
    cursor: pointer;
    color: #B4AFAC; 
    padding: 0;
    z-index: 2; 
    transition: all 0.2s ease;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .wishlist-btn:hover {
    transform: scale(1.1);
    color: #BB4E58; 
  }

  .wishlist-btn.is-favorite {
    color: #BB4E58; 
  }

  .wishlist-btn.is-favorite svg {
    fill: currentColor; 
  }

  .loading-text { 
    padding: 50px; 
    text-align: center; 
    font-size: 20px; 
  }

  .no-stock { 
    font-size: 20px; 
    color: #BB4E58; 
    font-weight: 600; 
    justify-self: center; 
  }

  .breadcrumbs { 
    color: #888; 
    margin-bottom: 25px; 
    font-size: 18px; 
  }

  .product-main { 
    display: flex; 
    justify-content: space-between; 
    align-items: center; 
    gap: 40px; 
    padding: 40px; 
    background: #fff; 
    border-radius: 40px; 
    margin-bottom: 50px; 
    box-shadow: 0 4px 20px rgba(0,0,0,0.03); 
  }

  .product-visual img { 
    width: 200px; 
    object-fit: contain; 
  }

  .product-details { 
    flex: 1; 
    display: flex; 
    flex-direction: column; 
    gap: 30px; 
  }

  .product-details h1 { 
    font-size: 28px; 
    font-weight: 700; 
    margin: 0; 
    line-height: 1.2; 
    word-wrap: break-word; 
    overflow-wrap: break-word; 
    word-break: break-word; 
  }

  .manufacturer { 
    color: #B4AFAC; 
    font-size: 18px; 
    margin: 5px 0; 
  }

  .prescription-status { 
    color: #689D6D; 
    font-size: 18px; 
    font-weight: 500; 
  }

  .prescription-status.is-red { 
    color: #BB4E58; 
  }

  .price-range { 
    font-size: 28px; 
    font-weight: 500; 
    color: #333; 
  }

  .disclaimer { 
    font-size: 14px; 
    color: #ccc; 
    margin: 0; 
  }

  .action-column { 
    display: flex; 
    flex-direction: column; 
    gap: 15px; 
    min-width: 200px; 
  }

  .btn-outline { 
    border: 2.5px solid #BB4E58; 
    color: #BB4E58; 
    background: #fff; 
    padding: 12px 25px; 
    border-radius: 20px; 
    font-weight: 600; 
    text-align: center; 
    cursor: pointer; 
    font-family: var(--main-font); 
    font-size: 20px; 
    width: 100%; 
    display: block; 
    box-sizing: border-box; 
  }

  .btn-outline:hover:not(:disabled) { 
    background: #fff5f6; 
  }

  .btn-outline:disabled { 
    border-color: #ccc; 
    color: #ccc; 
    cursor: not-allowed; 
  }

  .btn-primary { 
    background: #BB4E58; 
    color: #fff; 
    border: none; 
    padding: 14px 25px; 
    border-radius: 20px; 
    font-family: var(--main-font); 
    font-size: 20px; 
    cursor: pointer; 
    display: flex; 
    align-items: center; 
    justify-content: center; 
    gap: 10px; 
    width: 100%; 
  }

  .btn-primary:hover { 
    filter: brightness(0.9); 
  }

  .pdf-fullscreen-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background: #525659; 
    z-index: 9999; 
    display: flex;
    flex-direction: column;
  }

  .pdf-toolbar {
    height: 60px;
    background: #323639; 
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0 20px;
    box-shadow: 0 2px 5px rgba(0,0,0,0.2);
    flex-shrink: 0; 
  }

  .pdf-title {
    color: white;
    font-size: 16px;
    font-weight: 500;
    font-family: var(--main-font);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 70%;
  }

  .pdf-close-btn {
    background: #BB4E58;
    color: white;
    border: none;
    padding: 8px 16px;
    border-radius: 8px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    transition: 0.2s;
    font-family: var(--main-font);
  }

  .pdf-close-btn:hover {
    background: #9a3d46;
  }

  .pdf-iframe {
    flex: 1;
    width: 100%;
    height: 100%;
    border: none;
    background: white;
  }

  .page-num, .page-arrow { 
    width: 44px; 
    height: 44px; 
    display: flex; 
    align-items: center; 
    justify-content: center; 
    border-radius: 50%; 
    border: none; 
    cursor: pointer; 
    font-size: 16px; 
    font-weight: 600; 
    transition: all 0.2s ease; 
    background: #f0f0f0; 
    color: #333; 
  }

  .page-arrow { 
    color: #888; 
    background: #f0f0f0; 
  }

  .page-arrow:not(:disabled):hover { 
    background: #e0e0e0; 
    color: #000; 
  }

  .page-arrow:disabled { 
    cursor: not-allowed; 
    opacity: 0.5; 
  }

  .page-num.active { 
    background: #689D6D; 
    color: #fff; 
  }

  .page-num:not(.active):hover { 
    background: #e5e5e5; 
  }

  .pagination { 
    display: flex; 
    justify-content: center; 
    align-items: center; 
    gap: 12px; 
    margin: 40px 0; 
  }

  .pharmacies-section {
    background: white;
    border-radius: 30px;
    padding: 30px;
    margin-bottom: 50px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.03);
  }

  .pharmacies-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
    flex-wrap: wrap;
    gap: 15px;
  }

  .pharmacies-header h2 {
    margin: 0;
    font-size: 24px;
  }

  .view-tabs {
    display: flex;
    background: #f5f5f5;
    border-radius: 12px;
    padding: 4px;
  }

  .view-tabs button {
    border: none;
    background: transparent;
    padding: 8px 20px;
    border-radius: 8px;
    cursor: pointer;
    font-family: inherit;
    font-weight: 600;
    color: #666;
    font-size: 15px;
    transition: 0.2s;
  }

  .view-tabs button.active {
    background: white;
    color: var(--primary-color, #689D6D);
    box-shadow: 0 2px 5px rgba(0,0,0,0.1);
  }

  .filters-panel {
    display: flex;
    gap: 20px;
    padding: 15px 20px;
    border-radius: 15px;
    margin-bottom: 25px;
    flex-wrap: wrap;
  }

  .filter-group {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .filter-group label {
    font-weight: 600;
    color: #555;
    font-size: 14px;
  }

  .filter-select {
    padding: 8px 12px;
    border-radius: 8px;
    border: 1px solid #ddd;
    outline: none;
    font-family: inherit;
    font-size: 14px;
    min-width: 150px;
  }

  .filter-select:focus {
    border-color: var(--primary-color, #689D6D);
  }

  .map-view-container {
    width: 100%;
    height: 500px;
    border-radius: 20px;
    overflow: hidden;
    border: 1px solid #eee;
  }

  .yandex-map-block {
    width: 100%;
    height: 100%;
    position: relative;
  }

  .map-loading {
    position: absolute;
    top: 0; left: 0; right: 0; bottom: 0;
    background: rgba(255,255,255,0.8);
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: bold;
    color: #666;
    z-index: 100;
  }

  @media (max-width: 992px) {
    .product-main { 
      flex-direction: column; 
      align-items: flex-start; 
      padding: 30px; 
      gap: 30px; 
    }
    
    .product-visual { 
      align-self: center; 
    }
    
    .action-column { 
      width: 100%; 
      flex-direction: row; 
    }
    
    .btn-outline, .btn-primary { 
      flex: 1; 
    }
  }

  @media (max-width: 600px) {

    .breadcrumbs { 
      font-size: 14px; 
    }
    
    .product-page { 
      padding: 20px 10px; 
    }
    
    .product-main { 
      padding: 20px; 
      border-radius: 20px; 
    }
    
    .product-details h1 { 
      font-size: 22px; 
    }
    
    .price-range { 
      font-size: 26px; 
    }
    
    .product-visual img { 
      width: 150px; 
    }
    
    .action-column { 
      flex-direction: column; 
    }
    
    .btn-outline, .btn-primary { 
      width: 100%; 
      font-size: 16px; 
      padding: 12px; 
    }
    
    .pagination { 
      gap: 6px; 
      flex-wrap: wrap; 
    }
    
    .page-num, .page-arrow { 
      width: 38px; 
      height: 38px; 
      font-size: 14px; 
    }
    
    .pdf-title { 
      font-size: 14px; 
    }
    
    .pdf-close-btn { 
      font-size: 14px; 
      padding: 6px 12px; 
    }
  }
</style>