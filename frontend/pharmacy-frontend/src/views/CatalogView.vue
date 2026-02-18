<template>
  <TheHeader />
  <div class="catalog-page">
    <div class="container">
      <ProductList 
        :products="products" 
        :isLoading="isLoading" 
        :title="pageTitle" 
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import ProductList from '@/components/ProductList.vue';
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

    
</style>