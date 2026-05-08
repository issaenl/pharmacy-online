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

       <div class="filter-group">
          <h4 class="filter-title">Сортировка</h4>
          <div class="custom-select-wrapper" :style="{ zIndex: showSort ? 100 : 11 }" @click.stop>
            <div class="selected-option" @click="showSort ? closeSort() : (closeAllDropdowns(), toggleSort())">
              {{ selectedSortName }}
              <span class="arrow" :class="{ 'arrow-up': showSort }">▼</span>
            </div>
            <div v-if="showSort" class="dropdown-menu">
              <ul class="options-list">
                <li 
                  v-for="opt in filteredSort" 
                  :key="opt.id" 
                  @click="selectFilter('sortBy', opt.id)" 
                  :class="{ selected: filters.sortBy === opt.id }"
                >
                  {{ opt.name }}
                </li>
              </ul>
            </div>
          </div>

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
            <h4 class="filter-title">Наличие</h4>
            <div class="radio-group">
              <label><input type="radio" v-model="filters.inStock" value="all" /> Все товары</label>
              <label><input type="radio" v-model="filters.inStock" :value="true" /> Есть в наличии</label>
              <label><input type="radio" v-model="filters.inStock" :value="false" /> Нет в наличии</label>
            </div>
          </div>

          <div class="filter-group">
            <h4 class="filter-title">Категория</h4>
            <div class="custom-select-wrapper" :style="{ zIndex: showCat ? 100 : 10 }" @click.stop>
              <div class="selected-option" @click="showCat ? closeCat() : (closeAllDropdowns(), toggleCat())">
                {{ selectedCatName }}
                <span class="arrow" :class="{ 'arrow-up': showCat }">▼</span>
              </div>
              <div v-if="showCat" class="dropdown-menu">
                <input type="text" v-model="searchCat" placeholder="Поиск категории..." class="search-box" @click.stop />
                <ul class="options-list">
                  <li @click="selectFilter('categoryId', '')" :class="{ selected: !filters.categoryId }">Все категории</li>
                  <li v-for="cat in filteredCat" :key="cat.id" @click="selectFilter('categoryId', cat.id)" :class="{ selected: filters.categoryId === cat.id }">
                    {{ cat.name }}
                  </li>
                  <li v-if="filteredCat.length === 0" class="no-options">Категория не найдена</li>
                </ul>
              </div>
            </div>
          </div>

          <div class="filter-group">
            <h4 class="filter-title">Страна производства</h4>
            <div class="custom-select-wrapper" :style="{ zIndex: showCountry ? 100 : 9 }" @click.stop>
              <div class="selected-option" @click="showCountry ? closeCountry() : (closeAllDropdowns(), toggleCountry())">
                {{ selectedCountryName }}
                <span class="arrow" :class="{ 'arrow-up': showCountry }">▼</span>
              </div>
              <div v-if="showCountry" class="dropdown-menu">
                <input type="text" v-model="searchCountry" placeholder="Поиск страны..." class="search-box" @click.stop />
                <ul class="options-list">
                  <li @click="selectFilter('country', '')" :class="{ selected: !filters.country }">Любая страна</li>
                  <li v-for="c in filteredCountry" :key="c.id" @click="selectFilter('country', c.id)" :class="{ selected: filters.country === c.id }">
                    {{ c.name }}
                  </li>
                  <li v-if="filteredCountry.length === 0" class="no-options">Страна не найдена</li>
                </ul>
              </div>
            </div>
          </div>

          <div class="filter-group">
            <h4 class="filter-title">Производитель</h4>
            <div class="custom-select-wrapper" :style="{ zIndex: showMan ? 100 : 8 }" @click.stop>
              <div class="selected-option" @click="showMan ? closeMan() : (closeAllDropdowns(), toggleMan())">
                {{ selectedManName }}
                <span class="arrow" :class="{ 'arrow-up': showMan }">▼</span>
              </div>
              <div v-if="showMan" class="dropdown-menu">
                <input type="text" v-model="searchMan" placeholder="Поиск производителя..." class="search-box" @click.stop />
                <ul class="options-list">
                  <li @click="selectFilter('manufacturer', '')" :class="{ selected: !filters.manufacturer }">Любой производитель</li>
                  <li v-for="m in filteredMan" :key="m.id" @click="selectFilter('manufacturer', m.id)" :class="{ selected: filters.manufacturer === m.id }">
                    {{ m.name }}
                  </li>
                  <li v-if="filteredMan.length === 0" class="no-options">Производитель не найден</li>
                </ul>
              </div>
            </div>
          </div>

          <div class="filter-group">
            <h4 class="filter-title">Наличие в области</h4>
            <div class="custom-select-wrapper" :style="{ zIndex: showDist ? 100 : 7 }" @click.stop>
             <div class="selected-option" @click="showDist ? closeDist() : (closeAllDropdowns(), toggleDist())">
                {{ selectedDistName }}
                <span class="arrow" :class="{ 'arrow-up': showDist }">▼</span>
              </div>
              <div v-if="showDist" class="dropdown-menu">
                <input type="text" v-model="searchDist" placeholder="Поиск области..." class="search-box" @click.stop />
                <ul class="options-list">
                  <li @click="selectFilter('district', '')" :class="{ selected: !filters.district }">Любая область</li>
                  <li v-for="d in filteredDist" :key="d.id" @click="selectFilter('district', d.id)" :class="{ selected: filters.district === d.id }">
                    {{ d.name }}
                  </li>
                  <li v-if="filteredDist.length === 0" class="no-options">Область не найдена</li>
                </ul>
              </div>
            </div>
          </div>

          <button class="reset-btn" @click="resetFilters">Сбросить фильтры</button>
        </div>
      </aside>

      <div class="sidebar-overlay" v-if="isMobileFilterOpen" @click="isMobileFilterOpen = false"></div>

      <main class="catalog-content">
        <ProductList 
          :products="paginatedProducts" 
          :isLoading="isLoading" 
          :title="pageTitle" 
        />
        
        <AppPagination 
          v-if="!isLoading && totalPages > 1"
          v-model:currentPage="currentPage"
          :totalPages="totalPages"
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
import AppPagination from '@/components/AppPagination.vue'; 
import { useCustomSelect } from '@/logic/customSelect';
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
  district: '',
  inStock: 'all',
  sortBy: 'default'
});

const availableCategories = ref([]); 
const availableCountries = ref([]);
const availableManufacturers = ref([]);
const availableDistricts = ref([]);

const pageTitle = 'Каталог товаров';
const currentPage = ref(1);
const itemsPerPage = 30;

const sortOptions = ref([
  { id: 'default', name: 'По умолчанию' },
  { id: 'price_asc', name: 'Сначала дешевые' },
  { id: 'price_desc', name: 'Сначала дорогие' },
  { id: 'name_asc', name: 'По названию (А-Я)' },
  { id: 'name_desc', name: 'По названию (Я-А)' }
]);

const { 
  showDropdown: showSort, filteredItems: filteredSort, 
  toggleDropdown: toggleSort, closeDropdown: closeSort 
} = useCustomSelect(sortOptions, 'name');

const { 
  showDropdown: showCat, searchQuery: searchCat, filteredItems: filteredCat, 
  toggleDropdown: toggleCat, closeDropdown: closeCat 
} = useCustomSelect(availableCategories, 'name');

const { 
  showDropdown: showCountry, searchQuery: searchCountry, filteredItems: filteredCountry, 
  toggleDropdown: toggleCountry, closeDropdown: closeCountry 
} = useCustomSelect(availableCountries, 'name');

const { 
  showDropdown: showMan, searchQuery: searchMan, filteredItems: filteredMan, 
  toggleDropdown: toggleMan, closeDropdown: closeMan 
} = useCustomSelect(availableManufacturers, 'name');

const { 
  showDropdown: showDist, searchQuery: searchDist, filteredItems: filteredDist, 
  toggleDropdown: toggleDist, closeDropdown: closeDist 
} = useCustomSelect(availableDistricts, 'name');

const closeAllDropdowns = () => {
  closeSort(); closeCat(); closeCountry(); closeMan(); closeDist();
};

const selectedCatName = computed(() => availableCategories.value.find(c => c.id === filters.value.categoryId)?.name || 'Все категории');
const selectedCountryName = computed(() => filters.value.country || 'Любая страна');
const selectedManName = computed(() => filters.value.manufacturer || 'Любой производитель');
const selectedDistName = computed(() => filters.value.district || 'Любая область');
const selectedSortName = computed(() => sortOptions.value.find(o => o.id === filters.value.sortBy)?.name || 'По умолчанию');

const selectFilter = (field, value) => {
  filters.value[field] = value;
  closeAllDropdowns();
};

const totalPages = computed(() => Math.ceil(products.value.length / itemsPerPage));

const paginatedProducts = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage;
  const end = start + itemsPerPage;
  return products.value.slice(start, end);
});

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
      if (filters.value.inStock !== 'all') params.append('inStock', filters.value.inStock);
      if (filters.value.sortBy && filters.value.sortBy !== 'default') {
        params.append('sortBy', filters.value.sortBy);
      }

      response = await api.get(`/Products?${params.toString()}`);
    }
    products.value = response.data;
    currentPage.value = 1;
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

    availableCountries.value = countriesRes.data.map(i => ({ id: i, name: i }));
    availableManufacturers.value = manufacturersRes.data.map(i => ({ id: i, name: i }));
    availableDistricts.value = districtsRes.data.map(i => ({ id: i, name: i }));
    availableCategories.value = categoriesRes.data;
  } catch (error) {
    console.error("Ошибка загрузки списков фильтров:", error);
  }
};

const resetFilters = () => {
  filters.value = {
    priceMin: null, priceMax: null, prescription: 'all',
    categoryId: '', country: '', manufacturer: '', district: '', inStock: 'all',
    sortBy: 'default'
  };
};

watch(currentPage, () => { window.scrollTo({ top: 0, behavior: 'smooth' }); });

watch(filters, (newFilters) => {
  sessionStorage.setItem('catalogFilters', JSON.stringify(newFilters));
  fetchProducts();
}, { deep: true });

onMounted(() => {
  fetchProducts();
  fetchFilterOptions();
});

onBeforeRouteLeave((to, from, next) => {
  if (to.name !== 'product') sessionStorage.removeItem('catalogFilters');
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
      background: #f0f0f0;
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
        gap: 0;
      }

      .catalog-content {
        width: 100%;
        flex: none;
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

.radio-group input[type="radio"] {
  accent-color: #689D6D;
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.custom-select-wrapper .selected-option {
  background-color: #F5F5F5;
  border: none;
  border-radius: 10px;
  padding: 10px 12px;
  font-size: 16px;
  font-family: var(--main-font);
  color: #000;
}

.custom-select-wrapper .selected-option:hover {
  background-color: #ebebeb;
}

.custom-select-wrapper .dropdown-menu {
  top: 100%;
  bottom: auto;
  margin-top: 4px;
  margin-bottom: 0;
  border-radius: 10px;
  border: 1px solid #ddd;
  box-shadow: 0 4px 15px rgba(0,0,0,0.1);
  background: white;
}

.custom-select-wrapper .search-box {
  border-top: none;
  border-bottom: 1px solid #eee;
  border-radius: 10px 10px 0 0;
  background-color: white;
}

.custom-select-wrapper .options-list li {
  padding: 10px 15px;
  border-bottom: 1px solid #f9f9f9;
}

.custom-select-wrapper .options-list li:last-child {
  border-bottom: none;
}

.custom-select-wrapper .options-list li.selected {
  background-color: #689D6D;
  color: white;
  font-weight: normal;
}

.custom-select-wrapper .options-list li:hover {
  background-color: #f0f0f0;
  color: #000;
}

.custom-select-wrapper .options-list li.selected:hover {
  background-color: #5c8b60;
  color: white;
}
</style>