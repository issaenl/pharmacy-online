<template>
  <div class="admin-stocks">
    <div class="header-actions">
      <h2>Управление наличием</h2>
      
      <div class="filters-group">
        <select v-model="selectedPharmacyFilter" class="form-select filter-select">
          <option value="">Все аптеки</option>
          <option v-for="p in pharmacies" :key="p.id" :value="p.id">
            {{ p.name }} ({{ p.address }})
          </option>
        </select>

        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Поиск по товару..." 
          class="search-input" 
        />
      </div>

      <div class="action-buttons">
        <input 
          type="file" 
          ref="fileInput" 
          @change="handleFileUpload" 
          accept=".csv, .xlsx, .xls" 
          style="display: none;"/>
        <button class="btn-cancel" @click="$refs.fileInput.click()">Импорт</button>
        <button class="btn-primary" @click="openModal()">+ Добавить товар в наличие</button>
      </div>
    </div>

    <div v-if="importErrors.length > 0" class="import-errors-alert">
      <div class="error-header">
        <h4>Внимание! Некоторые строки не были загружены:</h4>
        <button class="btn-close-error" @click="importErrors = []">✕</button>
      </div>
      <ul class="error-list">
        <li v-for="(error, index) in importErrors" :key="index">{{ error }}</li>
      </ul>
    </div>

    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th v-if="!selectedPharmacyFilter" @click="sortBy('pharmacyName')" class="sortable">
              Аптека <span v-if="sortKey === 'pharmacyName'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('productName')" class="sortable">
              Товар <span v-if="sortKey === 'productName'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('quantity')" class="sortable">
              Остаток <span v-if="sortKey === 'quantity'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('price')" class="sortable">
              Цена <span v-if="sortKey === 'price'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Последнее обновление</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="stock in sortedAndFilteredStocks" :key="stock.id">
            <td v-if="!selectedPharmacyFilter" class="text-muted">{{ stock.pharmacyName }}</td>
            <td>{{ stock.productName }}</td>
            <td>
              <span :class="{'out-of-stock': stock.quantity === 0}">{{ stock.quantity }} шт.</span>
            </td>
            <td>{{ stock.price }} руб.</td>
            <td class="text-muted">{{ new Date(stock.lastUpdate).toLocaleString() }}</td>
            <td class="actions">
              <button class="btn-action" @click="openModal(stock)" title="Редактировать">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                  <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
                </svg>
              </button>
              <button class="btn-action" @click="deleteStock(stock.id)" title="Удалить">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="3 6 5 6 21 6"></polyline>
                  <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                </svg>
              </button>
            </td>
          </tr>
          <tr v-if="sortedAndFilteredStocks.length === 0">
            <td :colspan="selectedPharmacyFilter ? 5 : 6" class="text-center text-muted" style="padding: 30px;">
              Запасы не найдены
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <h3>{{ isEditing ? 'Редактировать наличие' : 'Добавить наличие' }}</h3>
        
        <form @submit.prevent="saveStock" class="pharmacy-form">
          <div class="form-grid">
            <div class="form-column">
                <label>Аптека
                    <select v-model="form.pharmacyId" required class="form-select" :disabled="isEditing">
                        <option value="" disabled>Выберите аптеку...</option>
                        <option v-for="p in pharmacies" :key="p.id" :value="p.id">{{ p.name }} ({{ p.address }})</option>
                    </select>
                </label>
                <label>Товар
                    <select v-model="form.productId" required class="form-select" :disabled="isEditing">
                        <option value="" disabled>Выберите товар...</option>
                        <option v-for="p in products" :key="p.id" :value="p.id">{{ p.name }} ({{ p.manufacturer }})</option>
                    </select>
                </label>
            </div>

            <div class="form-column">
                <label>Остаток (шт)
                  <input type="number" min="0" v-model="form.quantity" step="1" required />
                </label>
                <label>Цена (руб)
                  <input type="number" min="0.01" v-model="form.price" step="0.01" required />
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

const stocks = ref([]);
const pharmacies = ref([]);
const products = ref([]);

const showModal = ref(false);
const isLoading = ref(false);
const isEditing = ref(false);
const currentId = ref(null);
const fileInput = ref(null);
const importErrors = ref([]);

const searchQuery = ref('');
const selectedPharmacyFilter = ref('');
const sortKey = ref(null); 
const sortOrder = ref(0);

const form = ref({ pharmacyId: '', productId: '', quantity: 0, price: 0 });

const fetchData = async () => {
  try {
    const [stocksRes, pharmRes, prodRes] = await Promise.all([
      api.get('/Stocks/admin-all'),
      api.get('/Pharmacies'),
      api.get('/Products/admin-all-products')
    ]);
    stocks.value = stocksRes.data;
    pharmacies.value = pharmRes.data;
    products.value = prodRes.data;
  } catch (error) {
    toast.error("Ошибка загрузки данных");
  }
};

onMounted(() => { fetchData(); });

const handleFileUpload = async (event) => {
  const file = event.target.files[0];
  if (!file) return;

  const formData = new FormData();
  formData.append('file', file);

  try {
    toast.info("Загрузка файла...");
    const response = await api.post('/Stocks/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    
    toast.success(response.data.message);
    if (response.data.errors?.length > 0) {
      importErrors.value = response.data.errors;
    } else {
      importErrors.value = [];
    }
    await fetchData();
  } catch (error) {
    toast.error(error.response?.data?.message || "Ошибка импорта");
  } finally {
    event.target.value = '';
  }
};

const openModal = (stock = null) => {
  showModal.value = true;
  if (stock && stock.id) {
    isEditing.value = true;
    currentId.value = stock.id;
    form.value = { pharmacyId: stock.pharmacyId, productId: stock.productId, quantity: stock.quantity, price: stock.price };
  } else {
    isEditing.value = false;
    currentId.value = null;
    
    form.value = { 
      pharmacyId: selectedPharmacyFilter.value || '', 
      productId: '', 
      quantity: 1, 
      price: 100 
    };
  }
};

const closeModal = () => { showModal.value = false; };

const saveStock = async () => {
  isLoading.value = true;
  try {
    if (isEditing.value) {
      await api.put(`/Stocks/${currentId.value}`, form.value);
      toast.success("Запас обновлен!"); 
    } else {
      await api.post('/Stocks', form.value);
      toast.success("Запас добавлен!"); 
    }
    await fetchData();
    closeModal();
  } catch (error) {
    toast.error(error.response?.data?.message || "Ошибка сохранения");
  } finally {
    isLoading.value = false;
  }
};

const deleteStock = async (id) => {
  if (!confirm('Удалить этот запас?')) return;
  try {
    await api.delete(`/Stocks/${id}`);
    await fetchData();
    toast.success("Запас удален");
  } catch (error) {
    toast.error("Ошибка удаления");
  }
};

const sortBy = (key) => {
  if (sortKey.value === key) {
    if (sortOrder.value === 1) sortOrder.value = -1;
    else if (sortOrder.value === -1) { sortOrder.value = 0; sortKey.value = null; }
  } else { sortKey.value = key; sortOrder.value = 1; }
};


const sortedAndFilteredStocks = computed(() => {
  let result = stocks.value;

  if (selectedPharmacyFilter.value) {
    result = result.filter(s => s.pharmacyId === selectedPharmacyFilter.value);
  }

  if (searchQuery.value) {
    result = result.filter(s => 
      s.productName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      (!selectedPharmacyFilter.value && s.pharmacyName.toLowerCase().includes(searchQuery.value.toLowerCase()))
    );
  }

  if (sortKey.value && sortOrder.value !== 0) {
    result.sort((a, b) => {
      let valA = a[sortKey.value];
      let valB = b[sortKey.value];
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
.admin-stocks {
  padding: 20px;
}

.header-actions {
  display: flex;
  flex-direction: column;
  margin-bottom: 20px;
  gap: 15px;
}

.header-actions h2 {
  margin: 0;
}

.filters-group {
  display: flex;
  gap: 15px;
  width: 100%;
}

.filter-select {
  max-width: 300px;
}

.action-buttons {
  display: flex;
  gap: 10px;
  align-self: flex-start;
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

.form-select {
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-family: inherit;
  font-size: 14px;
  width: 100%;
  background-color: white;
  cursor: pointer;
}

.form-select:disabled {
  background-color: #f5f5f5;
  cursor: not-allowed;
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

th,
td {
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

.out-of-stock {
  color: #e74c3c;
  font-weight: bold;
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

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  margin-top: 30px;
}

.import-errors-alert {
  background-color: #fee2e2;
  border: 1px solid #fca5a5;
  border-left: 5px solid #BB4E58;
  border-radius: 8px;
  padding: 15px 20px;
  margin-bottom: 20px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

.error-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.error-header h4 {
  margin: 0;
  color: #BB4E58;
  font-size: 16px;
  font-family: inherit;
}

.btn-close-error {
  background: transparent;
  border: none;
  color: #BB4E58;
  font-size: 18px;
  font-weight: bold;
  cursor: pointer;
  padding: 0 5px;
  transition: opacity 0.2s;
  line-height: 1;
}

.btn-close-error:hover {
  opacity: 0.6;
}

.error-list {
  margin: 0;
  padding-left: 20px;
  color: #7f1d1d;
  font-size: 14px;
  max-height: 180px;
  overflow-y: auto;
}

.error-list li {
  margin-bottom: 6px;
  line-height: 1.4;
}
</style>