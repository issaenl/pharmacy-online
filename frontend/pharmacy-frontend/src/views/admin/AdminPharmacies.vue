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
            <th>Рейтинг</th>
            <th>Координаты</th>
            <th>Действия</th> </tr>
        </thead>
        <tbody>
          <tr v-for="pharmacy in sortedAndFilteredPharmacies" :key="pharmacy.id">
            <td>{{ pharmacy.name }}</td>
            <td>{{ pharmacy.district }}</td>
            <td>{{ pharmacy.address }}</td>
            <td>{{ pharmacy.phone }}</td>
            <td>
              <span class="rating-badge" v-if="pharmacy.rating">⭐ {{ pharmacy.rating }}</span>
              <span v-else class="text-muted">—</span>
            </td>
            <td class="coords">
              <span v-if="pharmacy.latitude && pharmacy.longitude">
                {{ pharmacy.latitude.toFixed(4) }}, {{ pharmacy.longitude.toFixed(4) }}
              </span>
              <span v-else class="text-muted">Нет данных</span>
            </td>
            <td class="actions">
              <button class="btn-action" @click="openModal(pharmacy)" title="Редактировать">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                  <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
                </svg>
              </button>
              <button class="btn-action" @click="deletePharmacy(pharmacy.id)" title="Удалить">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="3 6 5 6 21 6"></polyline>
                  <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                </svg>
              </button>
            </td>
          </tr>
          <tr v-if="sortedAndFilteredPharmacies.length === 0">
            <td colspan="7" class="text-center text-muted" style="padding: 30px;">
              Аптеки не найдены
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <h3>{{ isEditing ? 'Редактировать аптеку' : 'Добавить новую аптеку' }}</h3>
        
        <form @submit.prevent="savePharmacy" class="pharmacy-form">
          <div class="form-grid">
            <div class="form-column">
                <label>Название <input v-model="form.name" required placeholder="Аптека Плюс" /></label>
                <label>Область <input v-model="form.district" required placeholder="Гомельская" /></label>
                <label>Адрес <input v-model="form.address" required placeholder="ул. Ленина, д. 10" /></label>
                <label>Телефон <input v-model="form.phone" required placeholder="+375 (00) 000-00-00" /></label>
                <label>Рейтинг (0-5) 
                  <input type="number" step="0.1" min="0" max="5" v-model="form.rating" />
                </label>
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
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useToast } from 'vue-toast-notification';
import api from '@/api/api';

const toast = useToast({ position: 'bottom-right' });

const pharmacies = ref([]);
const showModal = ref(false);
const isLoading = ref(false);

const isEditing = ref(false);
const currentId = ref(null);

const searchQuery = ref('');
const sortKey = ref(null); 
const sortOrder = ref(0);

const form = ref({
  name: '', district: '', address: '', phone: '', rating: null, latitude: null, longitude: null
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
  showModal.value = true;
  
  if (pharmacy && pharmacy.id) {
    isEditing.value = true;
    currentId.value = pharmacy.id;
    form.value = { ...pharmacy };
  } else {
    isEditing.value = false;
    currentId.value = null;
    form.value = { name: '', district: '', address: '', phone: '', rating: null, latitude: null, longitude: null };
  }
};

const closeModal = () => { 
  showModal.value = false; 
};

const savePharmacy = async () => {
  isLoading.value = true;
  try {
    const payload = {
      name: form.value.name,
      address: form.value.address,
      district: form.value.district,
      phone: form.value.phone,
      rating: form.value.rating ? parseFloat(form.value.rating) : null,
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

const sortBy = (key) => {
  if (sortKey.value === key) {
    if (sortOrder.value === 1) {
        sortOrder.value = -1;
    }
    else if (sortOrder.value === -1) { 
        sortOrder.value = 0; sortKey.value = null; 
    }
  } 
  else {
    sortKey.value = key;
    sortOrder.value = 1;
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
</script>

<style scoped>
.admin-pharmacies {
  padding: 20px;
}

.header-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  gap: 15px;
  flex-wrap: wrap;
}

.search-input {
  flex: 1;
  max-width: 400px;
  padding: 10px 15px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 16px;
  font-family: inherit;
}

.search-input:focus {
  outline: none;
  border-color: var(--primary-color);
}

.table-container {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.05);
  overflow: hidden;
}

table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

th, td {
  padding: 15px;
  border-bottom: 1px solid #eee;
}

th {
  background: var(--background-color);
  color: #000;
  font-weight: 600;
}

.text-center {
  text-align: center;
}

.text-muted {
  color: #999;
  font-size: 14px;
}

.sortable {
  cursor: pointer;
  user-select: none;
  transition: background-color 0.2s;
}

.sortable:hover {
  background-color: #f5f5f5;
}

.sort-icon {
  font-size: 10px;
  margin-left: 5px;
  color: var(--primary-color);
}

.rating-badge {
  background: #fff8e1;
  color: #d97706;
  padding: 4px 8px;
  border-radius: 12px;
  font-weight: bold;
  font-size: 13px;
}

.btn-primary {
  background: var(--primary-color);
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: bold;
  font-size: 16px;
  font-family: var(--main-font);
}

.btn-primary:hover {
  opacity: 0.9;
}

.btn-primary:disabled {
  background: var(--primary-light);
  cursor: not-allowed;
}

.btn-cancel {
  background: #e0e0e0;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 16px;
  font-family: var(--main-font);
}

.actions {
  white-space: nowrap;
}

.btn-action {
  background: none;
  border: none;
  cursor: pointer;
  padding: 5px;
  border-radius: 6px;
  transition: 0.2s;
  margin-right: 5px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: #9e9e9e;
}

.btn-action:hover {
  background: rgba(158, 158, 158, 0.1);
  color: #616161;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  padding: 30px;
  border-radius: 15px;
  width: 90%;
  max-width: 800px;
  max-height: 90vh;
  overflow-y: auto;
}

.form-grid {
  display: flex;
  gap: 30px;
  margin-top: 20px;
}

.form-column {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.form-column label {
  display: flex;
  flex-direction: column;
  font-size: 14px;
  font-weight: bold;
  color: #333;
  gap: 5px;
}

.form-column input {
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-family: inherit;
  font-size: 15px;
}

.info-box {
  background: #e0f2fe;
  border-left: 4px solid #0284c7;
  padding: 12px;
  border-radius: 4px;
  color: #0369a1;
  font-size: 16px;
  line-height: 1.4;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  margin-top: 30px;
}
</style>