<template>
  <TheHeader />

  <main class="container">
    <section class="home-section">
      <div class="section-header">
        <h2>Новое в каталоге</h2>
        <router-link to="/new" class="view-all">Посмотреть всё →</router-link>
      </div>
      
      <div v-if="loading" class="loader">Загрузка...</div>
      <div v-else class="products-grid">
        <ProductCard 
          v-for="item in newProducts" 
          :key="item.id" 
          :product="item"
          @details="goToProduct"
        />
      </div>
    </section>

    <section class="home-section">
      <div class="section-header">
        <h2>Популярные товары</h2>
        <router-link to="/popular" class="view-all">Посмотреть всё →</router-link>
      </div>
      
      <div v-if="loading" class="loader">Загрузка...</div>
      <div v-else class="products-grid">
        <ProductCard 
          v-for="item in popularProducts" 
          :key="item.id" 
          :product="item"
          @details="goToProduct"
        />
      </div>
    </section>
  </main>
</template>

<script setup>
import api from '@/api/api';
import { ref, onMounted } from 'vue';
import ProductCard from '@/components/ProductCard.vue';
import TheHeader from '@/components/Header.vue';
import { useRouter } from 'vue-router';

const newProducts = ref([]);
const popularProducts = ref([]);
const loading = ref(true);
const router = useRouter();

const fetchData = async () => {
  try {
    loading.value = true;
    
    const [newRes, popularRes] = await Promise.allSettled([
      api.get('/Products/new'),
      api.get('/Products/popular')
    ]);
    
    if (newRes.status === 'fulfilled') {
      newProducts.value = newRes.value.data.slice(0, 4);
    } else {
      console.error("Ошибка загрузки новинок:", newRes.reason);
    }

    if (popularRes.status === 'fulfilled') {
      popularProducts.value = popularRes.value.data.slice(0, 4);
    } else {
      console.error("Ошибка загрузки популярных:", popularRes.reason);
    }

  } catch (error) {
    console.error("Ошибка загрузки:", error);
  } finally {
    loading.value = false;
  }
};

const goToProduct = (id) => {
  router.push(`/product/${id}`);
};

onMounted(fetchData);
</script>

<style>
  body{
    background: #F5F5F5;
  }

  .container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
  }

  .home-section {
    margin-bottom: 50px;
  }

  .section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
  }

  .section-header h2 {
    font-size: 24px;
    color: #333;
  }

  .view-all {
    color: #666;
    text-decoration: none;
    font-size: 20px;
  }

  .products-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 20px;
  }

  @media (max-width: 1024px) {
    .products-grid { grid-template-columns: repeat(2, 1fr); }
  }

  @media (max-width: 600px) {
    .products-grid { grid-template-columns: 1fr; }
  }
</style>