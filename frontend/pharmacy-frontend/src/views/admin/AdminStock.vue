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
            <th @click="sortBy('discountPercentage')" class="sortable">
              Скидка <span v-if="sortKey === 'discountPercentage'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('expirationDate')" class="sortable">
              Годен до <span v-if="sortKey === 'expirationDate'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Обновлено</th>
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
            <td>
              <div class="price-cell">
                 <span v-if="stock.discountPercentage" class="old-price">{{ stock.price.toFixed(2) }}</span>
                 <span :class="{'discounted-price': stock.discountPercentage}">{{ calculateFinalPrice(stock.price, stock.discountPercentage) }} руб.</span>
              </div>
            </td>
            
            <td>
              <div v-if="stock.discountPercentage" class="discount-wrapper">
                <span class="discount-badge">-{{ stock.discountPercentage }}%</span>
                <button class="reset-discount-btn" @click="resetDiscount(stock)" title="Сбросить скидку">✕</button>
              </div>
              <span v-else class="text-muted">—</span>
            </td>
            
            <td>
              <span :class="{'out-of-stock': new Date(stock.expirationDate) < new Date()}">
                {{ new Date(stock.expirationDate).toLocaleDateString('ru-RU') }}
              </span>
            </td>
            
            <td class="text-muted">{{ new Date(stock.lastUpdate).toLocaleDateString('ru-RU') }}</td>
            <TableActions 
                @edit="openModal(stock)" 
                @delete="deleteStock(stock.id)" />
          </tr>
          <tr v-if="sortedAndFilteredStocks.length === 0">
            <td :colspan="isPharmacyAdmin || selectedPharmacyFilter ? 6 : 7" class="text-center text-muted" style="padding: 30px;">
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
                <label>Срок годности партии
                  <input type="date" v-model="form.expirationDate" required />
                </label>
            </div>

            <div class="form-column">
                <label>Остаток (шт)
                  <input type="number" min="0" v-model="form.quantity" step="1" required />
                </label>
                
                <label>Оригинальная цена (руб)
                  <input type="number" min="0.01" v-model="form.price" step="0.01" required />
                </label>
                
                <label>Скидка (%) <span class="text-muted" style="font-weight: normal; font-size: 14px;">(0 если нет)</span>
                  <input type="number" min="0" max="99" v-model="form.discountPercentage" step="1" />
                </label>
                
                <div class="final-price-hint" v-if="form.price && form.discountPercentage > 0">
                  Итоговая цена для покупателя: <strong>{{ calculateFinalPrice(form.price, form.discountPercentage) }} руб.</strong>
                </div>
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
  price: 0,
  discountPercentage: 0, 
  expirationDate: ''
});

const calculateFinalPrice = (price, discountPercent) => {
  if (!discountPercent) return Number(price).toFixed(2);
  const discountAmount = price * (discountPercent / 100);
  return (price - discountAmount).toFixed(2);
};

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
      price: stock.price,
      discountPercentage: stock.discountPercentage || 0,
      expirationDate: stock.expirationDate ? stock.expirationDate.split('T')[0] : '' 
    };
  } else {
    form.value = { 
      pharmacyId: isPharmacyAdmin.value ? myPharmacyId : (selectedPharmacyFilter.value || ''), 
      productId: '', 
      quantity: 1, 
      price: 100,
      discountPercentage: 0,
      expirationDate: '' 
    };
  }
};

const saveStock = async () => {
  isLoading.value = true;
  try {
    
    const payload = {
        ...form.value,
        discountPercentage: form.value.discountPercentage > 0 ? form.value.discountPercentage : null
    };

    if (isEditing.value) {
      await api.put(`/Stocks/${currentId.value}`, payload);
      toast.success("Запас обновлен!"); 
    } else {
      await api.post('/Stocks', payload);
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

const resetDiscount = async (stock) => {
  if (!confirm('Сбросить скидку для этого товара?')) return;
  
  isLoading.value = true;
  try {
    const payload = {
      pharmacyId: stock.pharmacyId,
      productId: stock.productId,
      quantity: stock.quantity,
      price: stock.price,
      expirationDate: stock.expirationDate ? stock.expirationDate.split('T')[0] : '',
      discountPercentage: null
    };

    await api.put(`/Stocks/${stock.id}`, payload);
    toast.success("Скидка сброшена!");
    await fetchData();
  } catch (error) {
    toast.error(error.response?.data?.message || "Ошибка при сбросе скидки");
  } finally {
    isLoading.value = false;
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
      
      if (valA === null) valA = '';
      if (valB === null) valB = '';

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
  color: var(--accent-color, #BB4E58);
  font-weight: bold;
}

.error-list {
  color: #7f1d1d;
}

.price-cell {
    display: flex;
    flex-direction: column;
    justify-content: center;
}

.old-price {
    font-size: 12px;
    text-decoration: line-through;
    color: #999;
    line-height: 1;
}

.discounted-price {
    color: var(--accent-color, #BB4E58);
    font-weight: 600;
}

.discount-wrapper {
    display: flex;
    align-items: center;
    gap: 6px;
}

.discount-badge {
    background: #FDE8E8;
    color: var(--accent-color, #BB4E58);
    padding: 4px 8px;
    border-radius: 6px;
    font-size: 13px;
    font-weight: bold;
}

.reset-discount-btn {
    background: none;
    border: none;
    color: #A0A0A0;
    cursor: pointer;
    font-size: 14px;
    font-weight: bold;
    padding: 2px 4px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 4px;
    transition: 0.2s ease;
}

.reset-discount-btn:hover {
    color: var(--accent-color, #BB4E58);
    background: #FDE8E8;
}

.final-price-hint {
    background: #E8F4EA;
    color: #689D6D;
    padding: 10px;
    border-radius: 8px;
    font-size: 14px;
    margin-top: 10px;
}
</style>