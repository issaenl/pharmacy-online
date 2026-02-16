<template>
  <TheHeader />
  <div class="catalog-page">
    <div class="container">
      <header class="catalog-header">
        <h1 class="catalog-title">{{ pageTitle }}</h1>
        <p v-if="products.length" class="results-count">Найдено товаров: {{ products.length }}</p>
      </header>

      <div v-if="isLoading" class="loader-container">
        <div class="loader"></div>
        <p>Загрузка товаров...</p>
      </div>

      <div v-else-if="products.length > 0" class="products-grid">
        <ProductCard 
          v-for="product in products" 
          :key="product.id" 
          :product="product" 
        />
      </div>

      <div v-else class="empty-state">
        <h2>Ничего не нашли...</h2>
        <p>Попробуйте изменить запрос или выбрать другую категорию</p>
        <router-link to="/" class="back-home">На главную</router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import ProductCard from '@/components/ProductCard.vue';
import TheHeader from '@/components/Header.vue';
import api from '@/api/api';

const route = useRoute();

const products = ref([]);
const isLoading = ref(true);

const pageTitle = computed(() => {
  if (route.query.q) return `Поиск: "${route.query.q}"`;
  if (route.query.categoryName) return route.query.categoryName;
  return 'Каталог товаров';
});

const fetchProducts = async () => {
  isLoading.value = true;
  try {
    let response;
    if (route.query.q) {
      response = await api.get(`/Products/search?query=${encodeURIComponent(route.query.q)}`);
    } else {
      const url = route.query.categoryId 
        ? `/Products?categoryIds=${route.query.categoryId}` 
        : '/Products';
      response = await api.get(url);
    }
    products.value = response.data;
  } catch (error) {
    products.value = [];
  } finally {
    isLoading.value = false;
  }
};

watch(() => route.query, () => {
  fetchProducts();
}, { deep: true });

onMounted(fetchProducts);
</script>

<style scoped>
    .catalog-page {
        padding: 40px 0;
        background: #F5F5F5;
        min-height: 80vh;
    }

    .container {
        max-width: 1200px;
        margin: 0 auto;
        padding: 0 20px;
    }

    .catalog-header {
        margin-bottom: 30px;
    }

    .catalog-title {
        font-size: 32px;
        font-family: var(--main-font);
        color: #333;
    }

    .results-count {
        color: #888;
        margin-top: 5px;
    }

    .products-grid {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 20px;
    }

    .loader-container, .empty-state {
        text-align: center;
        padding: 100px 0;
    }

    .back-home {
        display: inline-block;
        margin-top: 20px;
        color: #689D6D;
        font-weight: 600;
        font-size: 24px;
    }


    @media (max-width: 1024px) {
        .products-grid
        { 
            grid-template-columns: repeat(3, 1fr); 
        }
    }

    @media (max-width: 768px) {
        .products-grid
        { 
            grid-template-columns: repeat(2, 1fr); 
        }
    }
</style>