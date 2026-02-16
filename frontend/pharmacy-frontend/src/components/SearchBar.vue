<template>
  <div class="search-container" ref="searchRef">
    <div class="search-bar">
      <div class="search-input-wrapper">
        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Поиск лекарств" 
          @focus="handleFocus"
          @keyup.enter="goToSearchPage"
        />
      </div>
      <button class="search-btn" @click="goToSearchPage">Найти</button>
    </div>

    <transition name="fade">
      <div v-if="showDropdown && (results.length || isLoading)" class="search-dropdown">
        <div v-if="isLoading" class="search-message">Ищем лучшие варианты...</div>
        
        <div v-else-if="results.length">
          <div 
            v-for="product in results.slice(0, 8)" 
            :key="product.id" 
            class="search-result-item"
            @click="handleSelect(product.id)"
          >
            <img :src="product.pictureUrl || '/assets/no-image.jpg'" class="item-img" />
            <div class="item-info">
              <span class="item-name">{{ product.name }}</span>
              <span class="item-dosage">{{ product.dosageForm }}</span>
            </div>
            <div class="item-price">от {{ product.minPrice }} р.</div>
          </div>
        </div>
        <div v-else class="search-message">Ничего не найдено</div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '@/api/api';

const router = useRouter();
const searchRef = ref(null);

const searchQuery = ref('');
const results = ref([]);
const isLoading = ref(false);
const showDropdown = ref(false);
let debounceTimer = null;

const performSearch = async () => {
  if (searchQuery.value.trim().length < 2) {
    results.value = [];
    showDropdown.value = false;
    return;
  }

  isLoading.value = true;
  showDropdown.value = true;

  try {
    const { data } = await api.get(`/Products/search?query=${encodeURIComponent(searchQuery.value)}`);
    results.value = data;
  } catch (error) {
    console.error("Search error:", error);
  } finally {
    isLoading.value = false;
  }
};

watch(searchQuery, () => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(performSearch, 300);
});

const handleFocus = () => {
  if (searchQuery.value.length >= 2) showDropdown.value = true;
};

const handleSelect = (id) => {
  showDropdown.value = false;
  searchQuery.value = '';
  router.push(`/product/${id}`);
};

const goToSearchPage = () => {
  if (!searchQuery.value.trim()) return;
  showDropdown.value = false;
  router.push({ path: '/catalog', query: { q: searchQuery.value } });
};

const handleClickOutside = (event) => {
  if (searchRef.value && !searchRef.value.contains(event.target)) {
    showDropdown.value = false;
  }
};

onMounted(() => document.addEventListener('click', handleClickOutside));
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

        .item-img 
        { 
            width: 35px; 
            height: 35px; 
        }
    }
</style>