<template>
  <div class="product-list-container">
    <header v-if="title || showCount" class="catalog-header">
      <h1 v-if="title" class="catalog-title">{{ title }}</h1>
      <p v-if="showCount && products.length" class="results-count">
        Найдено товаров: {{ products.length }}
      </p>
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
</template>

<script setup>
import ProductCard from '@/components/ProductCard.vue';

defineProps({
  products: { type: Array, required: true },
  isLoading: { type: Boolean, default: false },
  title: { type: String, default: '' },
  showCount: { type: Boolean, default: true }
});
</script>

<style scoped>
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
        grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
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