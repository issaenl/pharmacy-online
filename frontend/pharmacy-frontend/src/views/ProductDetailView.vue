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
          :class="{ 'is-favorite': isFavorite }" 
          @click="toggleFavorite"
          title="Добавить в избранное"
        >
          <svg 
            width="32" 
            height="32" 
            viewBox="0 0 24 24" 
            fill="none" 
            stroke="currentColor" 
            stroke-width="2" 
            stroke-linecap="round" 
            stroke-linejoin="round"
          >
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

    <div class="pdf-fullscreen-overlay" v-if="isPdfOpen">
      <div class="pdf-toolbar">
        <span class="pdf-title">Инструкция: {{ product.name }}</span>
        <button class="pdf-close-btn" @click="closeInstruction">
          Закрыть ✕
        </button>
      </div>
      
      <iframe 
        :src="product.pdfUrl" 
        class="pdf-iframe"
        title="Инструкция по применению"
      >
        <p>Ваш браузер не поддерживает предпросмотр PDF. <a :href="product.pdfUrl" target="_blank">Скачать файл</a></p>
      </iframe>
    </div>
    
    <div class="pharmacy-list">
      <div v-if="pharmacies.length === 0" class="no-stock">Товар временно отсутствует в аптеках!</div>
      <PharmacyItem 
        v-for="shop in paginatedPharmacies" 
        :key="shop.id" 
        :pharmacy="shop"
        @book="handleBookClick"/>
    </div>
    
    <div v-if="pharmacies.length > itemsPerPage" class="pagination">
      <button class="page-arrow" @click="setPage(currentPage - 1)" :disabled="currentPage === 1">‹</button>
      <button v-for="page in totalPages" :key="page" :class="['page-num', { active: currentPage === page }]" @click="setPage(page)">{{ page }}</button>
      <button class="page-arrow" @click="setPage(currentPage + 1)" :disabled="currentPage === totalPages">›</button>
    </div>

    <QuickOrderModal
      :is-open="isCheckoutModalOpen"
      :product="product"
      :pharmacy="selectedCheckoutPharmacy"
      :is-loading="isCheckoutLoading"
      @close="isCheckoutModalOpen = false"
      @confirm="submitQuickCheckout"/>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useCartStore } from '@/stores/cartStore';
import { useAuthStore } from '@/stores/authStore';
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
const orderStore = useOrderStore();
const toast = useToast({ position: 'bottom-right' });

const product = ref(null);
const pharmacies = ref([]);
const loading = ref(true);
const currentPage = ref(1);
const itemsPerPage = 10;

const isPdfOpen = ref(false);
const isCheckoutModalOpen = ref(false);
const selectedCheckoutPharmacy = ref(null);
const isCheckoutLoading = ref(false);

const isFavorite = ref(false);

const toggleFavorite = () => {
  isFavorite.value = !isFavorite.value;
};

const openInstruction = () => {
  isPdfOpen.value = true;
  document.body.style.overflow = 'hidden'; 
};

const closeInstruction = () => {
  isPdfOpen.value = false;
  document.body.style.overflow = ''; 
};

onUnmounted(() => {
  document.body.style.overflow = '';
});

const paginatedPharmacies = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage;
  const end = start + itemsPerPage;
  return pharmacies.value.slice(start, end);
});

const totalPages = computed(() => Math.ceil(pharmacies.value.length / itemsPerPage));

const setPage = (page) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page;
    window.scrollTo({ top: 400, behavior: 'smooth' }); 
  }
}

const formatPrice = (price) => {
  if (price == null) return '0.00';
  return Number(price).toFixed(2);
};

const addToCart = () => {
  if (!product.value) return;
  cartStore.addToCart({
    id: product.value.id,
    name: product.value.name,
    price: product.value.minPrice, 
    imageUrl: product.value.pictureUrl
  });
};

const handleBookClick = (pharmacy) => {
  if (!authStore.token) {
    router.push('/login');
    return;
  }
  selectedCheckoutPharmacy.value = pharmacy;
  isCheckoutModalOpen.value = true;
};

const submitQuickCheckout = async (quantity) => {
  if (!product.value || !selectedCheckoutPharmacy.value) return;

  isCheckoutLoading.value = true;
  const success = await orderStore.quickCheckout(
    product.value.id, 
    selectedCheckoutPharmacy.value.pharmacyId, 
    quantity
  );
  
  isCheckoutLoading.value = false;
  if (success) {
    isCheckoutModalOpen.value = false;
    router.push('/');
  }
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