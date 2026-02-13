<template>
  <div class="container" v-if="product">
    <button @click="$router.back()" class="back-btn">← Назад</button>

    <section class="product-detail card">
      <div class="product-detail__main">
        <img :src="product.pictureUrl || 'https://via.placeholder.com/200'" class="product-main-img" />
        <div class="product-main-info">
          <h1>{{ product.name }}</h1>
          <p class="manufacturer">{{ product.manufacturer }}, {{ product.country }}</p>
          <p class="manufacturer">{{ product.categoryName }}, {{ product.isPrescription }}</p>
          <div class="tags">
            <span class="tag">{{ product.dosageForm }}</span>
            <span class="tag" v-if="product.isPrescription">По рецепту</span>
          </div>
          <div class="description">
            <h3>Описание</h3>
            <p>{{ product.description || 'Инструкция по применению скоро будет добавлена.' }}</p>
          </div>
        </div>
      </div>
    </section>

    <section class="availability-section">
      <h2 class="section-title">Наличие в аптеках <span>{{ stocks.length }}</span></h2>
      
      <div v-if="loadingStocks" class="loader">Ищем в аптеках...</div>
      
      <div v-else-if="stocks.length">
        <PharmacyRow v-for="item in stocks" :key="item.pharmacyId" :stock="item" />
      </div>
      
      <div v-else class="empty-state">К сожалению, товара нет в наличии в ближайших аптеках.</div>
    </section>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import api from '@/api/api';
import PharmacyRow from '@/components/PharmacyRow.vue';

const route = useRoute();
const product = ref(null);
const stocks = ref([]);
const loadingStocks = ref(true);

const loadData = async () => {
  const id = route.params.id;
  try {
    const prodRes = await api.get(`/Products/${id}`);
    product.value = prodRes.data;

    const stockRes = await api.get(`/Products/${id}/availibility`);
    stocks.value = stockRes.data;
  } catch (e) {
    console.error("Ошибка загрузки данных", e);
  } finally {
    loadingStocks.value = false;
  }
};

onMounted(loadData);
</script>

<style scoped>
.container { max-width: 900px; margin: 0 auto; padding: 20px; }
.back-btn { background: none; border: none; color: #00a65a; cursor: pointer; margin-bottom: 20px; font-weight: bold; }

.product-detail { display: flex; flex-direction: column; padding: 30px; margin-bottom: 30px; background: white; border-radius: 16px; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
.product-detail__main { display: flex; gap: 40px; }
.product-main-img { width: 250px; height: 250px; object-fit: contain; }

.manufacturer { color: #888; margin-top: -10px; margin-bottom: 20px; }
.tags { display: flex; gap: 10px; margin-bottom: 20px; }
.tag { background: #eef2ff; color: #444; padding: 4px 12px; border-radius: 20px; font-size: 13px; }

.section-title { margin-top: 40px; font-size: 22px; display: flex; align-items: center; gap: 10px; }
.section-title span { background: #eee; font-size: 14px; padding: 2px 10px; border-radius: 10px; }
</style>