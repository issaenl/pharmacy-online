<template>
  <TheHeader />
  <div v-if="loading" class="container">Загрузка...</div>
  
  <div v-else-if="product" class="product-page container">
    <nav class="breadcrumbs">
      Каталог > {{ product.categoryName || 'Лекарства' }} > {{ product.name }}
    </nav>

    <section class="product-main card">
      <div class="product-visual">
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
          <div class="price-range">{{ product.minPrice }} — {{ product.maxPrice }} р.</div>
          <p class="disclaimer">Внешний вид товара может отличаться от изображенного на фотографии</p>
        </div>
      </div>

      <div class="action-column">
        <a v-if="product.pdfUrl" :href="product.pdfUrl" target="_blank" class="btn-outline instruction-btn">
          Инструкция <span class="arrow">></span>
        </a>
        <button v-else class="btn-outline instruction-btn" disabled>Инструкция ></button>
        
        <button class="btn-primary cart-btn">
          <img src="/assets/Cart.svg" alt="" class="cart-icon-white">
          В корзину
        </button>
      </div>
    </section>

    <div class="pharmacy-list">
      <div v-if="pharmacies.length === 0" class="no-stock">Товар временно отсутствует в аптеках</div>
      <PharmacyItem 
        v-for="shop in paginatedPharmacies" 
        :key="shop.id" 
        :pharmacy="shop" 
      />
    </div>
    
    <div v-if="pharmacies.length > itemsPerPage" class="pagination">
      <button 
        class="page-arrow" 
        @click="setPage(currentPage - 1)"
        :disabled="currentPage === 1"
      >‹</button>
      
      <button 
        v-for="page in totalPages" 
        :key="page"
        :class="['page-num', { active: currentPage === page }]"
        @click="setPage(page)"
      >
        {{ page }}
      </button>

      <button 
        class="page-arrow" 
        @click="setPage(currentPage + 1)"
        :disabled="currentPage === totalPages"
      >›</button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import PharmacyItem from '@/components/PharmacyItem.vue';
import TheHeader from '@/components/Header.vue';
import api from '@/api/api';

const route = useRoute();
const product = ref(null);
const pharmacies = ref([]);
const loading = ref(true);
const currentPage = ref(1);
const itemsPerPage = 1;

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
  .loading-text { 
    padding: 50px; 
    text-align: center; 
    font-size: 20px; 
  }
  
  .breadcrumbs { 
    color: #888; 
    margin-bottom: 25px; 
    font-size: 14px; 
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
    position: relative;
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
  
  .product-details h1 { font-size: 28px; 
    font-weight: 700; 
    margin: 0; 
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
    border-radius: 25px;
    font-weight: 600;
    text-decoration: none;
    text-align: center;
    cursor: pointer;
    font-family: var(--main-font);
    font-size: 20px;
  }

  .btn-primary {
    background: #BB4E58;
    color: #fff;
    border: none;
    padding: 14px 25px;
    border-radius: 25px;
    font-family: var(--main-font);
    font-size: 20px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
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
  }
</style>