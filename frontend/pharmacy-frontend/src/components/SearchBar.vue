<template>
  <div class="search-container" ref="searchRef">
    <div class="search-bar">
      <div class="search-input-wrapper">
        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Поиск лекарств и аптек" 
          @focus="handleFocus"
          @keyup.enter="goToSearchPage"
        />
      </div>
      <button class="search-btn" @click="goToSearchPage">Найти</button>
    </div>

    <transition name="fade">
      <div v-if="showDropdown && (productResults.length || pharmacyResults.length || isLoading || showHistoryCondition)" class="search-dropdown">
        
        <div v-if="showHistoryCondition" class="history-section">
          <div class="history-header">
            <span>Вы искали</span>
            <button @click.stop="clearHistory" class="clear-history-btn">Очистить</button>
          </div>
          <div 
            v-for="(item, index) in searchHistory" 
            :key="index" 
            class="history-item"
            @click="handleHistorySelect(item)"
          >
            <svg class="history-icon" viewBox="0 0 24 24" width="16" height="16">
              <path fill="currentColor" d="M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z"/>
            </svg>
            <span>{{ item }}</span>
          </div>
        </div>

        <template v-else-if="searchQuery.length >= 2">
          <div v-if="isLoading" class="search-message">Ищем лучшие варианты...</div>
          
          <div v-else-if="productResults.length || pharmacyResults.length">
            
            <div v-if="pharmacyResults.length" class="results-group">
              <div class="group-title">Аптеки</div>
              <div 
                v-for="pharmacy in pharmacyResults" 
                :key="'pharm-' + pharmacy.id" 
                class="search-result-item pharmacy-item-result"
                @click="handlePharmacySelect(pharmacy.id)"
              >
                <div class="pharmacy-icon-wrapper">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path><polyline points="9 22 9 12 15 12 15 22"></polyline></svg>
                </div>
                <div class="item-info">
                  <span class="item-name">{{ pharmacy.name }}</span>
                  <span class="item-dosage">{{ pharmacy.address }}</span>
                </div>
              </div>
            </div>

            <div v-if="productResults.length" class="results-group">
              <div class="group-title">Товары</div>
              <div 
                v-for="product in productResults.slice(0, 8)" 
                :key="'prod-' + product.id" 
                class="search-result-item"
                @click="handleProductSelect(product.id)"
              >
                <img :src="product.pictureUrl || '/assets/no-image.jpg'" class="item-img" />
                <div class="item-info">
                  <span class="item-name">{{ product.name }}</span>
                  <span class="item-dosage">{{ product.dosageForm }}</span>
                </div>
                <div class="item-price">от {{ product.minPrice }} р.</div>
              </div>
            </div>

          </div>
          <div v-else class="search-message">Ничего не найдено</div>
        </template>

      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, onUnmounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import api from '@/api/api';

const router = useRouter();
const searchRef = ref(null);

const searchQuery = ref('');
const productResults = ref([]);
const pharmacyResults = ref([]);
const isLoading = ref(false);
const showDropdown = ref(false);
const searchHistory = ref([]);
const MAX_HISTORY_ITEMS = 5;
let debounceTimer = null;

const showHistoryCondition = computed(() => {
  return searchQuery.value.trim().length < 2 && searchHistory.value.length > 0;
});

const loadHistory = () => {
  const saved = localStorage.getItem('searchHistory');
  if (saved) {
    try {
      searchHistory.value = JSON.parse(saved);
    } catch (e) {
      searchHistory.value = [];
    }
  }
};

const saveToHistory = (query) => {
  const q = query.trim();
  if (!q) return;

  searchHistory.value = searchHistory.value.filter(item => item !== q);
  searchHistory.value.unshift(q);

  if (searchHistory.value.length > MAX_HISTORY_ITEMS) {
    searchHistory.value.pop();
  }

  localStorage.setItem('searchHistory', JSON.stringify(searchHistory.value));
};

const clearHistory = () => {
  searchHistory.value = [];
  localStorage.removeItem('searchHistory');
};

const handleHistorySelect = (item) => {
  searchQuery.value = item;
  goToSearchPage(); 
};

const performSearch = async () => {
  if (searchQuery.value.trim().length < 2) {
    productResults.value = [];
    pharmacyResults.value = [];
    return;
  }

  isLoading.value = true;
  showDropdown.value = true;

  try {
    const [prodRes, pharmRes] = await Promise.all([
      api.get(`/Products/search?query=${encodeURIComponent(searchQuery.value)}`),
      api.get(`/Pharmacies/search?query=${encodeURIComponent(searchQuery.value)}`)
    ]);
    
    productResults.value = prodRes.data;
    pharmacyResults.value = pharmRes.data;
  } catch (error) {
    console.error(error);
  } finally {
    isLoading.value = false;
  }
};

watch(searchQuery, () => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(performSearch, 300);
});

const handleFocus = () => {
  if (searchQuery.value.length >= 2 || searchHistory.value.length > 0) {
    showDropdown.value = true;
  }
};

const handleProductSelect = (id) => {
  saveToHistory(searchQuery.value);
  showDropdown.value = false;
  searchQuery.value = '';
  router.push(`/product/${id}`);
};

const handlePharmacySelect = (id) => {
  saveToHistory(searchQuery.value);
  showDropdown.value = false;
  searchQuery.value = '';
  router.push(`/pharmacy/${id}`);
};

const goToSearchPage = () => {
  if (!searchQuery.value.trim()) return;
  saveToHistory(searchQuery.value);
  showDropdown.value = false;
  router.push({ path: '/catalog', query: { q: searchQuery.value } });
};

const handleClickOutside = (event) => {
  if (searchRef.value && !searchRef.value.contains(event.target)) {
    showDropdown.value = false;
  }
};

onMounted(() => {
  loadHistory(); 
  document.addEventListener('click', handleClickOutside);
});
onUnmounted(() => document.removeEventListener('click', handleClickOutside));
</script>

<style scoped>
    .search-container {
        flex: 1;
        min-width: 300px;
        position: relative;
    }

    .search-bar {
        display: flex;
        width: 100%;
    }

    .search-input-wrapper {
        position: relative;
        flex: 1;
    }

    .search-bar input {
        width: 100%;
        padding: 12px 20px;
        border: none;
        border-radius: 10px 0 0 10px;
        font-size: 18px;
        outline: none;
        font-family: var(--main-font);
        font-weight: 500;
    }

    .search-btn {
        background: #B3CCAE;
        color: white;
        border: none;
        padding: 0 25px;
        border-radius: 0 10px 10px 0;
        font-weight: 600;
        cursor: pointer;
        font-size: 18px;
        transition: background 0.2s;
        font-family: var(--main-font);
        font-weight: 500;
    }

    .search-btn:hover {
        background: #a2c09b;
    }

    .search-dropdown {
        position: absolute;
        top: calc(100% + 8px);
        left: 0;
        right: 0;
        background: white;
        border-radius: 12px;
        box-shadow: 0 10px 30px rgba(0,0,0,0.15);
        z-index: 1000;
        max-height: 450px;
        overflow-y: auto;
    }

    .search-message {
        padding: 20px;
        text-align: center;
        color: #888;
    }

    .history-section {
        padding: 10px 0;
    }

    .history-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 5px 20px 10px;
        font-size: 13px;
        color: #888;
        font-weight: 500;
    }

    .clear-history-btn {
        background: none;
        border: none;
        color: #B3CCAE;
        cursor: pointer;
        font-size: 13px;
        padding: 0;
        font: var(--main-font);
    }

    .clear-history-btn:hover {
        text-decoration: underline;
    }

    .history-item {
        display: flex;
        align-items: center;
        padding: 10px 20px;
        gap: 12px;
        cursor: pointer;
        color: #333;
        transition: background 0.2s;
    }

    .history-item:hover {
        background: #f9fbf9;
    }

    .history-icon {
        color: #ccc;
    }

    .results-group {
        padding-bottom: 10px;
    }

    .group-title {
        font-size: 12px;
        font-weight: 700;
        color: #888;
        text-transform: uppercase;
        padding: 15px 20px 5px;
        letter-spacing: 0.5px;
    }

    .search-result-item {
        display: flex;
        align-items: center;
        padding: 12px 20px;
        gap: 15px;
        cursor: pointer;
        border-bottom: 1px solid #f5f5f5;
        transition: background 0.2s;
    }

    .search-result-item:hover {
        background: #f9fbf9;
    }

    .pharmacy-icon-wrapper {
        width: 45px;
        height: 45px;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #E8F4EA;
        color: #689D6D;
        border-radius: 10px;
    }

    .item-img {
        width: 45px;
        height: 45px;
        object-fit: contain;
    }

    .item-info {
        flex: 1;
        display: flex;
        flex-direction: column;
    }

    .item-name {
        font-weight: 700;
        font-size: 16px;
    }

    .item-dosage {
        font-size: 13px;
        color: #999;
    }

    .item-price {
        font-weight: 600;
    }


    .fade-enter-active, .fade-leave-active 
    { 
        transition: opacity 0.2s; 
    }

    .fade-enter-from, .fade-leave-to 
    { 
        opacity: 0; 
    }


    @media (max-width: 992px) {
        .search-container {
            order: 3; 
            width: 100%;
            margin-top: 15px;
        }
    }

    @media (max-width: 600px) {
        .search-container {
            order: 3;
        }

        .search-bar input, .search-btn {
        font-size: 16px;
        padding: 10px;
        }
    
        .item-name 
        { 
            font-size: 14px; 
        }

        .item-price 
        { 
            font-size: 14px; 
        }

        .item-img, .pharmacy-icon-wrapper
        { 
            width: 35px; 
            height: 35px; 
        }
    }
</style>