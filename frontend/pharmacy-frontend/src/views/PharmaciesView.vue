<template>
  <TheHeader />
  
  <div class="pharmacies-page">
    <div class="container">
      <div class="page-header">
        <h2>Все аптеки</h2>
      </div>

      <div v-if="isLoading" class="loader">Загрузка...</div>

      <div v-else>
        <div v-if="pharmacies.length === 0" class="empty-state">
          К сожалению, список аптек пуст.
        </div>
        
        <div v-else class="pharmacies-list">
          <PharmacyCard 
            v-for="pharmacy in paginatedPharmacies" 
            :key="pharmacy.id" 
            :pharmacy="pharmacy" 
          />
        </div>

        <AppPagination 
          v-if="totalPages > 1"
          v-model:currentPage="currentPage"
          :totalPages="totalPages"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import api from '@/api/api';
import TheHeader from '@/components/Header.vue';
import PharmacyCard from '@/components/PharmacyCard.vue';
import AppPagination from '@/components/AppPagination.vue';

const pharmacies = ref([]);
const isLoading = ref(true);


const currentPage = ref(1);
const itemsPerPage = 20;

const totalPages = computed(() => {
  return Math.ceil(pharmacies.value.length / itemsPerPage);
});

const paginatedPharmacies = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage;
  const end = start + itemsPerPage;
  return pharmacies.value.slice(start, end);
});

const fetchPharmacies = async () => {
  isLoading.value = true;
  try {
    const response = await api.get('/Pharmacies');
    pharmacies.value = response.data;
    currentPage.value = 1;
  } catch (error) {
    console.error("Ошибка загрузки списка аптек:", error);
    pharmacies.value = [];
  } finally {
    isLoading.value = false;
  }
};

watch(currentPage, () => {
  window.scrollTo({ top: 0, behavior: 'smooth' });
});

onMounted(fetchPharmacies);
</script>

<style scoped>
.pharmacies-page {
  padding: 40px 0;
  background: #F5F5F5;
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
  color: #333;
  margin: 0;
}

.loader {
  text-align: center;
  padding: 50px;
  color: #888;
  font-size: 18px;
}

.empty-state {
  text-align: center;
  padding: 50px;
  background: #fff;
  border-radius: 20px;
  color: #888;
  font-size: 18px;
}

.pharmacies-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
  margin-bottom: 40px;
}
</style>