<template>
  <div class="admin-orders">
    <div class="header-actions">
      <h2>Управление заказами</h2>
      
      <div class="filters-group">
        <select v-model="selectedPharmacyFilter" class="form-select filter-select">
          <option value="">Все аптеки</option>
          <option v-for="p in pharmacies" :key="p.id" :value="p.id">
            {{ p.name }} ({{ p.address }})
          </option>
        </select>

        <select v-model="selectedStatusFilter" class="form-select filter-select">
          <option value="">Все статусы</option>
          <option v-for="(name, value) in statusNames" :key="value" :value="value">
            {{ name }}
          </option>
        </select>

        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Поиск по № заказа..." 
          class="search-input" 
        />
      </div>
    </div>

    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th @click="sortBy('id')" class="sortable">
              № <span v-if="sortKey === 'id'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('orderDate')" class="sortable">
              Дата <span v-if="sortKey === 'orderDate'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th v-if="!selectedPharmacyFilter" @click="sortBy('pharmacyName')" class="sortable">
              Аптека <span v-if="sortKey === 'pharmacyName'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('totalPrice')" class="sortable">
              Сумма <span v-if="sortKey === 'totalPrice'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Статус</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="order in paginatedOrders" :key="order.id">
            <td><strong>{{ order.id }}</strong></td>
            <td>{{ new Date(order.orderDate).toLocaleString() }}</td>
            <td v-if="!selectedPharmacyFilter" class="text-muted">{{ order.pharmacyName }}</td>
            <td><strong>{{ order.totalPrice.toFixed(2) }} руб.</strong></td>
            <td>
              <select 
                v-model="order.status" 
                @change="updateOrderStatus(order.id, $event.target.value)"
                class="status-select"
                :class="getStatusClass(order.status)"
              >
                <option v-for="(name, value) in statusNames" :key="value" :value="Number(value)">
                  {{ name }}
                </option>
              </select>
            </td>
            <td class="actions">
              <button class="btn-action" @click="openDetails(order)" title="Состав заказа">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                  <circle cx="12" cy="12" r="3"></circle>
                </svg>
              </button>
            </td>
          </tr>
          <tr v-if="sortedAndFilteredOrders.length === 0">
            <td :colspan="selectedPharmacyFilter ? 5 : 6" class="text-center text-muted" style="padding: 30px;">
              Заказы не найдены
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
      :title="`Состав заказа №${selectedOrder?.id || ''}`"
      @close="closeModal">
        <div v-if="selectedOrder" class="order-details">
          <div class="info-row"><strong>Аптека:</strong> {{ selectedOrder.pharmacyName }} ({{ selectedOrder.pharmacyAddress }})</div>
          <div class="info-row"><strong>Клиент:</strong> {{ selectedOrder.userFirstName }} ({{ selectedOrder.userPhone }})</div>
          <div class="info-row"><strong>Дата:</strong> {{ new Date(selectedOrder.orderDate).toLocaleString() }}</div>
          
          <h4 class="items-title">Товары:</h4>
          <table class="items-table">
            <thead>
              <tr>
                <th>Название</th>
                <th>Кол-во</th>
                <th>Цена</th>
                <th>Сумма</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in selectedOrder.items" :key="item.productId">
                <td>{{ item.productName }}</td>
                <td>{{ item.quantity }} шт.</td>
                <td>{{ item.price.toFixed(2) }} руб.</td>
                <td><strong>{{ (item.price * item.quantity).toFixed(2) }} руб.</strong></td>
              </tr>
            </tbody>
          </table>
          
          <div class="total-row">
            Итого: <strong>{{ selectedOrder.totalPrice.toFixed(2) }} руб.</strong>
          </div>
        </div>
        
        <div class="modal-actions">
          <button type="button" class="btn-primary" @click="closeModal">Закрыть</button>
        </div>
    </Modal>
  </div>
</template>

<script setup>
import Modal from '@/components/admin/Modal.vue';
import TablePagination from '@/components/admin/TablePagination.vue';
import { usePagination } from '@/logic/pagination';
import { useModal } from '@/logic/modal';
import { useSorting } from '@/logic/sorting';
import { ref, computed, onMounted } from 'vue';
import { useToast } from 'vue-toast-notification';
import api from '@/api/api';

const toast = useToast({ position: 'bottom-right' });

const { sortKey, sortOrder, sortBy } = useSorting();
const { showModal, openBaseModal, closeModal } = useModal();

const orders = ref([]);
const pharmacies = ref([]);
const selectedOrder = ref(null);

const searchQuery = ref('');
const selectedPharmacyFilter = ref('');
const selectedStatusFilter = ref('');

const statusNames = {
  0: 'Ожидает сборки',
  1: 'Готов к выдаче',
  2: 'Выполнен',
  3: 'Отменен',
  4: 'Истек срок'
};

const fetchData = async () => {
  try {
    const [ordersRes, pharmRes] = await Promise.all([
      api.get('/Orders/admin-all'),
      api.get('/Pharmacies')
    ]);
    orders.value = ordersRes.data;
    pharmacies.value = pharmRes.data;
  } catch (error) {
    toast.error("Ошибка загрузки данных");
  }
};

onMounted(() => { fetchData(); });

const updateOrderStatus = async (id, newStatus) => {
  try {
    await api.put(`/Orders/${id}/status`, { status: Number(newStatus) });
    toast.success("Статус заказа обновлен");
  } catch (error) {
    toast.error("Ошибка при обновлении статуса");
    await fetchData();
  }
};

const openDetails = (order) => {
  selectedOrder.value = order;
  openBaseModal();
};

const getStatusClass = (status) => {
  switch (Number(status)) {
    case 0: return 'status-pending';
    case 1: return 'status-ready';
    case 2: return 'status-completed';
    case 3: 
    case 4: return 'status-cancelled';
    default: return '';
  }
};

const sortedAndFilteredOrders = computed(() => {
  let result = orders.value;

  if (selectedPharmacyFilter.value) {
    result = result.filter(o => o.pharmacyId === selectedPharmacyFilter.value);
  }

  if (selectedStatusFilter.value !== '') {
    result = result.filter(o => o.status === Number(selectedStatusFilter.value));
  }

  if (searchQuery.value) {
    result = result.filter(o => String(o.id).includes(searchQuery.value));
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
  } else {
    result.sort((a, b) => new Date(b.orderDate) - new Date(a.orderDate));
  }
  
  return result;
});

const { 
  currentPage, 
  totalPages, 
  paginatedData: paginatedOrders,
  nextPage, 
  prevPage 
} = usePagination(sortedAndFilteredOrders, 15);
</script>

<style scoped>
.admin-orders {
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
  min-width: 150px;
  max-width: 250px;
}

.search-input {
  max-width: 200px;
}

.status-select {
  padding: 6px 10px;
  border-radius: 6px;
  font-family: inherit;
  font-size: 14px;
  font-weight: bold;
  cursor: pointer;
  border: 1px solid transparent;
  outline: none;
}

.status-select option {
  background-color: #ffffff;
  color: #333333;
}
    .status-pending {
        background: #fcf1d6;
        color: #F3C301;
    }

    .status-ready {
        background: #E8F4EA;
        color: #689D6D;
    }

    .status-completed {
        background: #F0F0F0;
        color: #333;
    }

    .status-cancelled {
        background: #FDE8E8;
        color: #BB4E58;
    }

    .status-expired {
        background: #F5F5F5;
        color: #888;
    }

.order-details {
  font-size: 15px;
}

.info-row {
  margin-bottom: 8px;
}

.items-title {
  margin: 20px 0 10px;
  padding-bottom: 5px;
  border-bottom: 1px solid #eee;
}

.items-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 20px;
}

.items-table th, .items-table td {
  padding: 10px;
  border-bottom: 1px solid #eee;
  text-align: left;
}

.items-table th {
  background-color: #f9fafb;
}

.total-row {
  text-align: right;
  font-size: 18px;
  margin-top: 15px;
}
</style>