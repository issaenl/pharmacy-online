<template>
  <div class="admin-stocks">
    <div class="header-actions">
      <h2>Управление наличием</h2>
      
      <div class="filters-group">
        <select v-if="!isPharmacyAdmin" v-model="selectedPharmacyFilter" class="form-select filter-select">
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

    <ErrorAlert :errors="importErrors" @clear="importErrors = []" />

    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th v-if="!isPharmacyAdmin && !selectedPharmacyFilter" @click="sortBy('pharmacyName')" class="sortable">
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
          <tr v-for="stock in paginatedStocks" :key="stock.id">
            <td v-if="!isPharmacyAdmin && !selectedPharmacyFilter" class="text-muted">{{ stock.pharmacyName }}</td>
            <td>{{ stock.productName }}</td>
            <td>
              <span :class="{'out-of-stock': stock.quantity === 0}">{{ stock.quantity }} шт.</span>
            </td>
            <td>{{ stock.price }} руб.</td>
            <td class="text-muted">{{ new Date(stock.lastUpdate).toLocaleString() }}</td>
            <TableActions 
                @edit="openModal(stock)" 
                @delete="deleteStock(stock.id)" />
          </tr>
          <tr v-if="sortedAndFilteredStocks.length === 0">
            <td :colspan="isPharmacyAdmin || selectedPharmacyFilter ? 5 : 6" class="text-center text-muted" style="padding: 30px;">
              Запасы не найдены
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
      :title="isEditing ? 'Редактировать запас' : 'Новый запас'"
      @close="closeModal">
        <form @submit.prevent="saveStock" class="pharmacy-form">
          <div class="form-grid">
            <div class="form-column">
                <label>Аптека
                    <select v-model="form.pharmacyId" required class="form-select" :disabled="isEditing || isPharmacyAdmin">
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
    </Modal>
  </div>
</template>

<script setup>
import Modal from '@/components/admin/Modal.vue';
import ErrorAlert from '@/components/admin/ErrorAlert.vue';
import TableActions from '@/components/admin/TableActions.vue';
import TablePagination from '@/components/admin/TablePagination.vue';
import { usePagination } from '@/logic/pagination';
import { useModal } from '@/logic/modal';
import { useSorting } from '@/logic/sorting';
import { ref, computed, onMounted } from 'vue';
import { useToast } from 'vue-toast-notification';
import { useAuthStore } from '@/stores/authStore';
import api from '@/api/api';

const toast = useToast({ position: 'bottom-right' });
const authStore = useAuthStore();

const isPharmacyAdmin = computed(() => authStore.user?.role === 1 || authStore.user?.role === 'PharmacyAdmin' || authStore.user?.roleName === 'PharmacyAdmin');
const myPharmacyId = authStore.user?.pharmacyId ? Number(authStore.user.pharmacyId) : '';

const { sortKey, sortOrder, sortBy } = useSorting();
const { showModal, isEditing, currentId, openBaseModal, closeModal } = useModal();

const stocks = ref([]);
const pharmacies = ref([]);
const products = ref([]);

const isLoading = ref(false);
const fileInput = ref(null);
const importErrors = ref([]);

const selectedPharmacyFilter = ref(isPharmacyAdmin.value ? myPharmacyId : '');
const searchQuery = ref('');

const form = ref({
  pharmacyId: isPharmacyAdmin.value ? myPharmacyId : '', 
  productId: '', 
  quantity: 0, 
  price: 0
});

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
    importErrors.value = error.response?.data?.errors || [];
    toast.error(error.response?.data?.message || "Ошибка импорта");
  } finally {
    event.target.value = '';
  }
};

const openModal = (stock = null) => {
  openBaseModal(stock?.id);

  if (stock && stock.id) {
    form.value = { 
      pharmacyId: stock.pharmacyId, 
      productId: stock.productId, 
      quantity: stock.quantity, 
      price: stock.price 
    };
  } else {
    form.value = { 
      pharmacyId: isPharmacyAdmin.value ? myPharmacyId : (selectedPharmacyFilter.value || ''), 
      productId: '', 
      quantity: 1, 
      price: 100 
    };
  }
};

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

const sortedAndFilteredStocks = computed(() => {
  let result = stocks.value;

  if (selectedPharmacyFilter.value !== '') {
    result = result.filter(s => s.pharmacyId === Number(selectedPharmacyFilter.value));
  }

  if (searchQuery.value) {
    result = result.filter(s => 
      s.productName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      (!isPharmacyAdmin.value && selectedPharmacyFilter.value === '' && s.pharmacyName.toLowerCase().includes(searchQuery.value.toLowerCase()))
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

const { 
  currentPage, 
  totalPages, 
  paginatedData: paginatedStocks,
  nextPage, 
  prevPage 
} = usePagination(sortedAndFilteredStocks, 15);
</script>

<style scoped>
.admin-stocks {
  padding: 20px;
}

.header-actions {
  display: flex;
  flex-direction: row;
  align-items: center;
  flex-wrap: wrap;
  gap: 15px;
  margin-bottom: 20px;
}

.header-actions h2 {
  margin: 0;
  white-space: nowrap;
}

.filters-group {
  display: flex;
  align-items: center;
  gap: 15px;
  flex: 1; 
  min-width: 300px;
  flex-wrap: wrap;
}

.filter-select {
  flex: 1;
  min-width: 200px;
  max-width: 300px;
}

.out-of-stock {
  color: #e74c3c;
  font-weight: bold;
}

.error-list {
  color: #7f1d1d;
}
</style>