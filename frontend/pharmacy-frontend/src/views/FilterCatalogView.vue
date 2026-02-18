<template>
  <TheHeader />
  <div class="catalog-page">
    <div class="container catalog-layout">
      
      <button class="mobile-filter-toggle" @click="isMobileFilterOpen = true">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 3H2l8 9.46V19l4 2v-8.54L22 3z"/></svg>
        Фильтры
      </button>

      <aside class="catalog-sidebar" :class="{ 'is-open': isMobileFilterOpen }">
        <div class="sidebar-header">
          <h3>Фильтры</h3>
          <button class="close-btn" @click="isMobileFilterOpen = false">✕</button>
        </div>

        <div class="filters-content">
          <div class="filter-group">
            <h4 class="filter-title">Цена</h4>
            <div class="price-inputs">
              <input type="number" v-model.number="filters.priceMin" placeholder="От" min="0" />
              <span class="price-separator">-</span>
              <input type="number" v-model.number="filters.priceMax" placeholder="До" min="0" />
            </div>
          </div>

          <div class="filter-group">
            <h4 class="filter-title">Рецепт</h4>
            <div class="radio-group">
              <label><input type="radio" v-model="filters.prescription" value="all" /> Все товары</label>
              <label><input type="radio" v-model="filters.prescription" :value="false" /> Без рецепта</label>
              <label><input type="radio" v-model="filters.prescription" :value="true" /> По рецепту</label>
            </div>
          </div>

          <div class="filter-group">
            <h4 class="filter-title">Категория</h4>
            <select v-model="filters.categoryId" class="filter-select">
              <option value="">Все категории</option>
              <option v-for="cat in availableCategories" :key="cat.id" :value="cat.id">
                {{ cat.name }}
              </option>
            </select>
          </div>

          <div class="filter-group">
            <h4 class="filter-title">Страна производства</h4>
            <select v-model="filters.country" class="filter-select">
              <option value="">Любая страна</option>
              <option v-for="country in availableCountries" :key="country" :value="country">
                {{ country }}
              </option>
            </select>
          </div>

          <div class="filter-group">
            <h4 class="filter-title">Производитель</h4>
            <select v-model="filters.manufacturer" class="filter-select">
              <option value="">Любой производитель</option>
              <option v-for="man in availableManufacturers" :key="man" :value="man">
                {{ man }}
              </option>
            </select>
          </div>

          <div class="filter-group">
            <h4 class="filter-title">Наличие в районе</h4>
            <select v-model="filters.district" class="filter-select">
              <option value="">Любой район</option>
              <option v-for="district in availableDistricts" :key="district" :value="district">
                {{ district }}
              </option>
            </select>
          </div>

          <button class="reset-btn" @click="resetFilters">Сбросить фильтры</button>
        </div>
      </aside>

      <div class="sidebar-overlay" v-if="isMobileFilterOpen" @click="isMobileFilterOpen = false"></div>

      <main class="catalog-content">
        <ProductList 
          :products="products" 
          :isLoading="isLoading" 
          :title="pageTitle" 
        />
      </main>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, onBeforeRouteLeave } from 'vue-router';
import ProductList from '@/components/ProductList.vue';
import TheHeader from '@/components/Header.vue';
import api from '@/api/api';

const route = useRoute();
const products = ref([]);
const isLoading = ref(true);
const isMobileFilterOpen = ref(false);

const savedFilters = sessionStorage.getItem('catalogFilters');

const filters = ref(savedFilters ? JSON.parse(savedFilters) : {
  priceMin: null,
  priceMax: null,
  prescription: 'all',
  categoryId: '',
  country: '',
  manufacturer: '',
  district: ''
});

const availableCategories = ref([]); 
const availableCountries = ref([]);
const availableManufacturers = ref([]);
const availableDistricts = ref([]);

const pageTitle = 'Каталог товаров';


const fetchProducts = async () => {
  isLoading.value = true;
  try {
    let response;
    if (route.query.q) {
      response = await api.get(`/Products/search?query=${encodeURIComponent(route.query.q)}`);
    } else {
      const params = new URLSearchParams();
      if (filters.value.categoryId) params.append('categoryIds', filters.value.categoryId);
      if (filters.value.priceMin) params.append('priceMin', filters.value.priceMin);
      if (filters.value.priceMax) params.append('priceMax', filters.value.priceMax);
      if (filters.value.prescription !== 'all') params.append('isPrescription', filters.value.prescription);
      if (filters.value.country) params.append('country', filters.value.country);
      if (filters.value.manufacturer) params.append('manufacturer', filters.value.manufacturer);
      if (filters.value.district) params.append('district', filters.value.district);

      response = await api.get(`/Products?${params.toString()}`);
    }
    products.value = response.data;
  } catch (error) {
    products.value = [];
  } finally {
    isLoading.value = false;
  }
};

const fetchFilterOptions = async () => {
  try {
    const [countriesRes, manufacturersRes, districtsRes, categoriesRes] = await Promise.all([
      api.get('/Filter/countries'),
      api.get('/Filter/manufacturers'),
      api.get('/Filter/districts'),
      api.get('/Categories')
    ]);

    availableCountries.value = countriesRes.data;
    availableManufacturers.value = manufacturersRes.data;
    availableDistricts.value = districtsRes.data;
    availableCategories.value = categoriesRes.data;
    
  } catch (error) {
    console.error("Ошибка загрузки списков фильтров:", error);
  }
};


const resetFilters = () => {
  filters.value = {
    priceMin: null,
    priceMax: null,
    prescription: 'all',
    categoryId: '',
    country: '',
    manufacturer: '',
    district: ''
  };
};


watch(filters, (newFilters) => {
  sessionStorage.setItem('catalogFilters', JSON.stringify(newFilters));
  fetchProducts();
}, { deep: true });

onMounted(() => {
  fetchProducts();
  fetchFilterOptions();
});

onBeforeRouteLeave((to, from, next) => {
  if (to.name !== 'product') {
    sessionStorage.removeItem('catalogFilters');
  }
  next();
});
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

    .catalog-layout {
      display: flex;
      gap: 30px;
      align-items: flex-start;
    }

    .mobile-filter-toggle {
      display: none;
      align-items: center;
      gap: 8px;
      background: #689D6D;
      color: white;
      border: none;
      padding: 12px 20px;
      border-radius: 15px;
      font-size: 18px;
      font-weight: 600;
      font-family: var(--main-font);
      cursor: pointer;
      margin-bottom: 20px;
      width: 100%;
      justify-content: center;
    }

    .catalog-sidebar {
      flex: 0 0 280px;
      background: white;
      padding: 24px;
      border-radius: 20px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.05);
      position: sticky;
      top: 20px;
    }

    .sidebar-header {
      display: none;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      padding-bottom: 15px;
      border-bottom: 1px solid #eee;
    }

    .sidebar-header h3 {
      margin: 0;
      font-size: 24px;
    }

    .close-btn {
      background: none;
      border: none;
      font-size: 20px;
      cursor: pointer;
      color: #888;
    }

    .filter-group {
      margin-bottom: 24px;
    }

    .filter-title {
      font-size: 20px;
      font-weight: 600;
      margin-bottom: 12px;
      color: #000;
    }

    .price-inputs {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .price-inputs input {
      width: 100%;
      padding: 8px 12px;
      background-color: #F5F5F5;
      border: none;
      border-radius: 10px;
      font-family: var(--main-font);
      font-size: 16px;
    }

    .price-separator {
      color: #888;
    }

    .radio-group {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .radio-group label {
      display: flex;
      align-items: center;
      gap: 8px;
      cursor: pointer;
      font-size: 16px;
      color: #000;
    }

    .filter-select {
      width: 100%;
      padding: 10px;
      background-color: #F5F5F5;
      border: none;
      border-radius: 10px;
      font-size: 16px;
      font-family: var(--main-font);
      cursor: pointer;
    }

    .reset-btn {
      width: 100%;
      padding: 12px;
      background: #d6d6d6;
      color: #000;
      border: none;
      border-radius: 15px;
      font-weight: 600;
      font-family: var(--main-font);
      font-size: 20px;
      cursor: pointer;
      transition: 0.2s;
    }

    .reset-btn:hover {
      background: #B4AFAC;
    }

    .catalog-content {
      flex: 1;
      min-width: 0;
    }

    .sidebar-overlay {
      display: none;
    }


    @media (max-width: 992px) {
      .catalog-layout {
        flex-direction: column;
      }

      .mobile-filter-toggle {
        display: flex;
      }

      .catalog-sidebar {
        position: fixed;
        top: 0;
        left: -100%;
        width: 300px;
        height: 100vh;
        z-index: 1000;
        margin: 0;
        border-radius: 0;
        overflow-y: auto;
        transition: left 0.3s ease;
      }

      .catalog-sidebar.is-open {
        left: 0;
      }

      .sidebar-header {
        display: flex;
      }

      .sidebar-overlay {
        display: block;
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        background: rgba(0,0,0,0.5);
        z-index: 999;
      }
    }
</style>