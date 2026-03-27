<template>
  <div class="admin-users">
    <div class="header-actions">
      <h2>Покупатели</h2>
      
      <div class="filters-group">
        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Поиск по имени, телефону или email..." 
          class="search-input" 
        />
      </div>
    </div>

    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th @click="sortBy('firstName')" class="sortable">
              Имя <span v-if="sortKey === 'firstName'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('phone')" class="sortable">
              Телефон <span v-if="sortKey === 'phone'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Email</th>
            <th @click="sortBy('ordersCount')" class="sortable text-center">
              Заказов <span v-if="sortKey === 'ordersCount'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('isBanned')" class="sortable">
              Статус <span v-if="sortKey === 'isBanned'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="customer in paginatedCustomers" :key="customer.id" :class="{'banned-row': customer.isBanned}">
            <td><strong>{{ customer.firstName }} {{ customer.lastName || '' }}</strong></td>
            <td>{{ customer.phone }}</td>
            <td class="text-muted">{{ customer.email || '—' }}</td>
            <td class="text-center font-bold">{{ customer.ordersCount }}</td>
            <td>
              <span class="role-badge" :class="customer.isBanned ? 'status-banned' : 'status-active'">
                {{ customer.isBanned ? 'Заблокирован' : 'Активен' }}
              </span>
            </td>
            
            <td class="actions-cell">
              <button 
                class="icon-action-btn" 
                :class="customer.isBanned ? 'hover-unban' : 'hover-ban'"
                @click="toggleBan(customer)"
                :title="customer.isBanned ? 'Разблокировать' : 'Заблокировать'">
                <svg v-if="customer.isBanned" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
                <path d="M7 11V7a5 5 0 0 1 9.9-1"></path>
                </svg>
                <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/></svg>
              </button>

              <button 
                class="icon-action-btn hover-reset" 
                @click="resetPassword(customer)"
                title="Сбросить пароль">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="23 4 23 10 17 10"></polyline>
                <path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"></path>
                </svg>
              </button>

              <button 
                class="icon-action-btn hover-delete" 
                @click="deleteCustomer(customer)"
                title="Удалить аккаунт навсегда">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/></svg>
              </button>
            </td>
            
          </tr>
          <tr v-if="sortedAndFilteredCustomers.length === 0">
            <td colspan="6" class="text-center text-muted" style="padding: 30px;">
              Покупатели не найдены
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
  </div>
</template>

<script setup>
import TablePagination from '@/components/admin/TablePagination.vue';
import { usePagination } from '@/logic/pagination';
import { useSorting } from '@/logic/sorting';
import { ref, computed, onMounted } from 'vue';
import { useToast } from 'vue-toast-notification';
import api from '@/api/api';

const toast = useToast({ position: 'bottom-right' });
const { sortKey, sortOrder, sortBy } = useSorting();

const customers = ref([]);
const searchQuery = ref('');

const fetchCustomers = async () => {
  try {
    const response = await api.get('/Users/customers');
    customers.value = response.data;
  } catch (error) {
    toast.error("Ошибка загрузки списка клиентов");
  }
};

onMounted(() => { fetchCustomers(); });

const toggleBan = async (customer) => {
  const actionText = customer.isBanned ? 'разблокировать' : 'заблокировать';
  if (!confirm(`Вы уверены, что хотите ${actionText} клиента ${customer.firstName}?`)) return;

  try {
    const response = await api.put(`/Users/ban/${customer.id}`);
    toast.success(response.data.message);
    await fetchCustomers();
  } catch (error) {
    toast.error(error.response?.data || "Произошла ошибка");
  }
};

const resetPassword = async (customer) => {
  if (!confirm(`Сбросить пароль для клиента ${customer.firstName} до значения по умолчанию?`)) return;

  try {
    const response = await api.put(`/Users/reset-customer-password/${customer.id}`);
    toast.success("Пароль успешно сброшен!");
  } catch (error) {
    toast.error(error.response?.data?.message || "Ошибка при сбросе пароля");
  }
};

const deleteCustomer = async (customer) => {
  if (!confirm(`Вы собираетесь навсегда удалить клиента ${customer.firstName} (тел: ${customer.phone}). Это действие удалит его корзину и избранное.\n\nПродолжить?`)) return;

  try {
    const response = await api.delete(`/Users/delete-customer/${customer.id}`);
    toast.success(response.data.message);
    await fetchCustomers();
  } catch (error) {
    toast.error(error.response?.data || "Ошибка при удалении пользователя. Возможно, к нему привязаны активные заказы.");
  }
};

const sortedAndFilteredCustomers = computed(() => {
  let result = customers.value;

  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase();
    result = result.filter(c => 
      c.firstName.toLowerCase().includes(q) ||
      (c.lastName && c.lastName.toLowerCase().includes(q)) ||
      (c.email && c.email.toLowerCase().includes(q)) ||
      c.phone.includes(q)
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
  paginatedData: paginatedCustomers,
  nextPage, 
  prevPage 
} = usePagination(sortedAndFilteredCustomers, 15);
</script>

<style scoped>
.admin-users { padding: 20px; }

.header-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  flex-wrap: wrap;
  gap: 15px;
}

.filters-group { flex: 1; max-width: 400px; }
.search-input { width: 100%; }

.font-bold { font-weight: 700; color: #333; }

.role-badge {
  padding: 5px 10px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 600;
}

.status-active { 
    background: #E8F4EA;
    color: #689D6D; 
}

.status-banned { 
    background: #FDE8E8;
    color: #BB4E58;
}

.banned-row td {
  background-color: #fffafa;
}


.actions-cell {
  display: flex;
  gap: 12px;
  justify-content: flex-start;
  align-items: center;
}

.icon-action-btn {
  background: none;
  border: none;
  padding: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #A0A0A0;
  transition: color 0.2s ease, transform 0.1s ease;
}

.icon-action-btn:hover {
  transform: scale(1.1);
}

.icon-action-btn.hover-ban:hover { color: #BB4E58; }
.icon-action-btn.hover-unban:hover { color: #689D6D; }
.icon-action-btn.hover-reset:hover { color: #d97706; }
.icon-action-btn.hover-delete:hover { color: #BB4E58; }
</style>