<template>
  <TheHeader />
  <div class="favorites-page">
    <div class="container">
      <div class="page-header">
        <h2>Избранное</h2>
      </div>

      <div v-if="favoriteStore.loading" class="loading-state">
        Загрузка...
      </div>

      <div v-else-if="favoriteStore.items.length === 0" class="empty-state">
        <p>В избранном пока нет товаров</p>
        <router-link to="/catalog" class="go-catalog-btn">В каталог</router-link>
      </div>

      <div v-else class="favorites-grid">
        <ProductCard 
          v-for="item in favoriteStore.items" 
          :key="item.productId" 
          :product="{
            id: item.productId,
            name: item.productName,
            minPrice: item.price,
            pictureUrl: item.pictureUrl,
            dosageForm: item.dosageForm 
          }" 
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue';
import { useFavoriteStore } from '@/stores/favoriteStore';
import TheHeader from '@/components/Header.vue';
import ProductCard from '@/components/ProductCard.vue';

const favoriteStore = useFavoriteStore();

onMounted(async () => {
  await favoriteStore.loadFavorites();
});
</script>

<style scoped>
    .favorites-page {
        padding: 40px 0;
        background: var(--background-color);
        min-height: 80vh;
        font-family: var(--main-font);
    }
  
    .container {
        max-width: 1200px;
        margin: 0 auto;
        padding: 0 20px;
    }

    .page-header {
        margin-bottom: 30px;
    }

    .page-header h2 {
        font-size: 28px;
        margin: 0;
        color: #000;
    }

    .favorites-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
        gap: 20px;
    }

    .empty-state, .loading-state {
        text-align: center;
        padding: 50px 0;
        color: #888;
        font-size: 18px;
    }

    .go-catalog-btn {
        display: inline-block;
        margin-top: 15px;
        background: var(--primary-color);
        color: white;
        font-weight: medium;
        font-size: 20px;
        text-decoration: none;
        padding: 10px 20px;
        border-radius: 8px;
    }

    .go-catalog-btn:hover {
        filter: brightness(0.9);
    }

</style>