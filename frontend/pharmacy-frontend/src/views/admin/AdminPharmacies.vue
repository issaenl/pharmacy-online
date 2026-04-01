<template>
  <div class="admin-pharmacies">
    <div class="header-actions">
      <h2>Управление аптеками</h2>
      <input 
        type="text" 
        v-model="searchQuery" 
        placeholder="Поиск по названию или адресу..." 
        class="search-input" 
      />
      <button class="btn-primary" @click="openModal()">+ Добавить аптеку</button>
    </div>

    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th @click="sortBy('name')" class="sortable">
              Название <span v-if="sortKey === 'name'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('district')" class="sortable">
              Район <span v-if="sortKey === 'district'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Адрес</th>
            <th>Телефон</th>
            <th @click="sortBy('rating')" class="sortable text-center">
              Рейтинг <span v-if="sortKey === 'rating'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Координаты</th>
            <th>Действия</th> 
          </tr>
        </thead>
        <tbody>
          <tr v-for="pharmacy in paginatedPharmacies" :key="pharmacy.id">
            <td>{{ pharmacy.name }}</td>
            <td>{{ pharmacy.district }}</td>
            <td>{{ pharmacy.address }}</td>
            <td>{{ pharmacy.phone }}</td>
            
            <td class="text-center">
              <RatingBadge v-if="pharmacy.rating" :rating="pharmacy.rating" />
              <span v-else class="text-muted">—</span>
            </td>
            
            <td class="coords">
              <span v-if="pharmacy.latitude && pharmacy.longitude">
                {{ pharmacy.latitude.toFixed(4) }}, {{ pharmacy.longitude.toFixed(4) }}
              </span>
              <span v-else class="text-muted">Нет данных</span>
            </td>
            <TableActions 
                @edit="openModal(pharmacy)" 
                @delete="deletePharmacy(pharmacy.id)" />
          </tr>
          <tr v-if="sortedAndFilteredPharmacies.length === 0">
            <td colspan="7" class="text-center text-muted" style="padding: 30px;">
              Аптеки не найдены
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <TablePagination 
      :current-page="currentPage" 
      :total-pages="totalPages" 
      @prev="prevPage" 
      @next="nextPage" />

    <Modal 
      :show="showModal" 
      :title="isEditing ? 'Редактировать аптеку' : 'Новая аптека'"
      @close="closeModal">
        <form @submit.prevent="savePharmacy" class="pharmacy-form">
          <div class="form-grid">
            <div class="form-column">
                <label>Название <input v-model="form.name" required placeholder="Аптека Плюс" /></label>
                <label>Область <input v-model="form.district" required placeholder="Гомельская" /></label>
                <label>Адрес <input v-model="form.address" required placeholder="ул. Ленина, д. 10" /></label>
                <label>Телефон <input v-model="form.phone" required placeholder="+375 (00) 000-00-00" /></label>
            </div>

            <div class="form-column">
                <div class="info-box">
                  <strong>О координатах:</strong><br/>
                  Можно оставить пустными. Если адрес указан верно, координаты заполнятся автоматически.
                </div>
                <label>Широта (Latitude) 
                  <input type="number" step="any" v-model="form.latitude" placeholder="55.7558" />
                </label>
                <label>Долгота (Longitude) 
                  <input type="number" step="any" v-model="form.longitude" placeholder="37.6173" />
                </label>
            </div>
          </div>

          <div class="modal-actions">
            <button type="button" class="btn-cancel" @click="closeModal">Отмена</button>
            <button type="submit" class="btn-primary" :disabled="isLoading">
              {{ isLoading ? 'Сохранение...' : 'Сохранить' }}
            </button>
          </div>
        </form>
     </Modal>
  </div>
</template>

<script setup>
import Modal from '@/components/admin/Modal.vue';
import TableActions from '@/components/admin/TableActions.vue';
import TablePagination from '@/components/admin/TablePagination.vue';
import RatingBadge from '@/components/RatingBadge.vue';
import { usePagination } from '@/logic/pagination';
import { useModal } from '@/logic/modal';
import { useSorting } from '@/logic/sorting';
import { ref, computed, onMounted } from 'vue';
import { useToast } from 'vue-toast-notification';
import api from '@/api/api';

const toast = useToast({ position: 'bottom-right' });

const { sortKey, sortOrder, sortBy } = useSorting();
const { showModal, isEditing, currentId, openBaseModal, closeModal } = useModal();

const pharmacies = ref([]);
const isLoading = ref(false);
const searchQuery = ref('');

const form = ref({
  name: '', district: '', address: '', phone: '', latitude: null, longitude: null
});

const fetchPharmacies = async () => {
  try {
    const response = await api.get('/Pharmacies');
    pharmacies.value = response.data;
  } 
  catch (error) {
    toast.error("Ошибка загрузки аптек");
  }
};

onMounted(() => {
  fetchPharmacies();
});

const openModal = (pharmacy = null) => {
  openBaseModal(pharmacy?.id);
  
  if (pharmacy && pharmacy.id) {
    form.value = { ...pharmacy };
  } else {
    form.value = { 
      name: '', 
      district: '', 
      address: '', 
      phone: '', 
      latitude: null, 
      longitude: null 
    };
  }
};

const savePharmacy = async () => {
  isLoading.value = true;
  try {
    const payload = {
      name: form.value.name,
      address: form.value.address,
      district: form.value.district,
      phone: form.value.phone,
      latitude: form.value.latitude ? parseFloat(form.value.latitude) : null,
      longitude: form.value.longitude ? parseFloat(form.value.longitude) : null
    };

    if (isEditing.value) {
      await api.put(`/Pharmacies/${currentId.value}`, payload);
      toast.success("Аптека успешно обновлена!"); 
    } else {
      await api.post('/Pharmacies', payload);
      toast.success("Аптека успешно добавлена!"); 
    }
    
    await fetchPharmacies();
    closeModal();
  } 
  catch (error) {
    let errorMsg = error.response?.data?.message || error.response?.data || error.message;
    if (typeof errorMsg === 'object') {
        errorMsg = JSON.stringify(errorMsg);
    }
    toast.error("Ошибка:\n" + errorMsg);
  } 
  finally {
    isLoading.value = false;
  }
};

const deletePharmacy = async (id) => {
  if (!confirm('Вы уверены, что хотите удалить эту аптеку?')) {
    return;
  }
  try {
    await api.delete(`/Pharmacies/${id}`);
    await fetchPharmacies();
    toast.success("Аптека успешно удалена");
  } 
  catch (error) {
    toast.error("Ошибка удаления");
  }
};

const sortedAndFilteredPharmacies = computed(() => {
  let result = pharmacies.value.filter(p => 
    p.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
    p.address.toLowerCase().includes(searchQuery.value.toLowerCase())
  );

  if (sortKey.value && sortOrder.value !== 0) {
    result.sort((a, b) => {
      let valA = a[sortKey.value] || '';
      let valB = b[sortKey.value] || '';
      if (typeof valA === 'string') valA = valA.toLowerCase();
      if (typeof valB === 'string') valB = valB.toLowerCase();
      
      if (valA < valB) return -1 * sortOrder.value;
      if (valA > valB) return 1 * sortOrder.value;
      return 0;
    });
  }
  return result;
});

const { 
  currentPage, 
  totalPages, 
  paginatedData: paginatedPharmacies,
  nextPage, 
  prevPage 
} = usePagination(sortedAndFilteredPharmacies, 15);
</script>

<style scoped>
.admin-pharmacies {
  padding: 20px;
}
.text-center {
  text-align: center;
}
</style>