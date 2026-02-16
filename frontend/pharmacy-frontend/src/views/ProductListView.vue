<template>
  <TheHeader />
  <main class="container">
    <div class="list-header">
      <router-link to="/" class="back-link">← Назад на главную</router-link>
      <h1>{{ title }}</h1>
    </div>

    <div v-if="loading" class="loader">Загрузка...</div>
    
    <div v-else-if="products.length === 0" class="empty-msg">
      Товары не найдены
    </div>

    <div v-else class="products-grid-full">
      <ProductCard 
        v-for="item in products" 
        :key="item.id" 
        :product="item"
        @details="goToProduct"
      />
    </div>
  </main>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import api from '@/api/api';
import ProductCard from '@/components/ProductCard.vue';
import TheHeader from '@/components/Header.vue';

const route = useRoute();
const router = useRouter();
const products = ref([]);
const loading = ref(true);

const title = computed(() => {
  return route.path.includes('new') ? 'Новинки каталога' : 'Популярные товары';
});

const fetchData = async () => {
  try {
    loading.value = true;
    const endpoint = route.path.includes('new') ? '/Products/new' : '/Products/popular';
    
    const res = await api.get(endpoint);
    
    products.value = res.data;
  } catch (error) {
    console.error("Ошибка загрузки списка:", error);
  } finally {
    loading.value = false;
  }
};

const goToProduct = (id) => {
  router.push(`/product/${id}`);
};

onMounted(fetchData);
</script>

<style scoped>
  .list-header {
    margin-bottom: 30px;
  }

  .back-link {
    color: #666;
    text-decoration: none;
    font-size: 20px;
    display: block;
    margin-bottom: 10px;
  }

  .products-grid-full {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 25px;
  }

  .empty-msg {
    text-align: center;
    padding: 50px;
    color: #666;
  }

  @media (max-width: 1024px) {
    .products-grid-full 
    { 
      grid-template-columns: repeat(3, 1fr); 
    }
  }

  @media (max-width: 768px) {
    .products-grid-full 
    { 
      grid-template-columns: repeat(2, 1fr); 
    }
  }
</style>