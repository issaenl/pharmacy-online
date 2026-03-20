<template>
  <div class="admin-users">
    <div class="header-actions">
      <h2>Сотрудники (Администраторы)</h2>
      
      <div class="filters-group">
        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Поиск по имени, email или телефону..." 
          class="search-input" 
        />
      </div>

      <div class="action-buttons">
        <button class="btn-primary" @click="openModal()">+ Добавить сотрудника</button>
      </div>
    </div>

    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th @click="sortBy('firstName')" class="sortable">
              Имя <span v-if="sortKey === 'firstName'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('email')" class="sortable">
              Email <span v-if="sortKey === 'email'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Телефон</th>
            <th @click="sortBy('role')" class="sortable">
              Роль <span v-if="sortKey === 'role'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Аптека</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="admin in paginatedAdmins" :key="admin.id">
            <td><strong>{{ admin.firstName }} {{ admin.lastName || '' }}</strong></td>
            <td>{{ admin.email }}</td>
            <td>{{ admin.phone }}</td>
            <td>
              <span class="role-badge" :class="{'role-super': admin.role === 2, 'role-pharmacy': admin.role === 1}">
                {{ admin.role === 2 ? 'Общий администратор' : 'Администратор аптеки' }}
              </span>
            </td>
            <td class="text-muted">{{ admin.pharmacyName || '—' }}</td>
            <td>
              <TableActions 
                @edit="openModal(admin)" 
                @delete="deleteAdmin(admin)" 
                :hide-delete="admin.email === 'admin@unimed.pharmacy'"
              />
            </td>
          </tr>
          <tr v-if="sortedAndFilteredAdmins.length === 0">
            <td colspan="6" class="text-center text-muted" style="padding: 30px;">
              Сотрудники не найдены
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
      :title="isEditing ? 'Редактировать профиль' : 'Новый сотрудник'"
      @close="closeModal">
        <form @submit.prevent="saveAdmin" class="pharmacy-form">
          <div class="form-grid">
            <div class="form-column">
                <label>Имя
                  <input type="text" v-model="form.firstName" required />
                </label>
                <label>Фамилия
                  <input type="text" v-model="form.lastName" />
                </label>
                <label>Телефон
                  <input type="text" :value="form.phone" @input="handlePhoneInput" placeholder="+375291234567" required />
                </label>
            </div>

            <div class="form-column">
                <label>Email
                  <input type="email" v-model="form.email" required :disabled="isEditing && form.email === 'admin@unimed.pharmacy'" />
                </label>
                
                <label v-if="!isEditing">Пароль
                  <input type="password" v-model="form.password" required minlength="8" placeholder="Минимум 8 символов" />
                </label>

                <div v-else-if="isEditing && form.email !== 'admin@unimed.pharmacy'" class="reset-password-wrapper">
                  <label>Пароль</label>
                  <button type="button" class="btn-cancel" @click="handleResetPassword(currentId)">
                    Сбросить пароль
                  </button>
                </div>

                <label>Роль
                    <select v-model="form.role" required class="form-select" :disabled="isEditing && form.email === 'admin@unimed.pharmacy'">
                        <option :value="2">Главный администратор</option>
                        <option :value="1">Администратор аптеки</option>
                    </select>
                </label>

                <label v-if="form.role === 1">Привязка к аптеке
                    <select v-model="form.pharmacyId" required class="form-select">
                        <option value="" disabled>Выберите аптеку...</option>
                        <option v-for="p in pharmacies" :key="p.id" :value="p.id">{{ p.name }}</option>
                    </select>
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
import { usePagination } from '@/logic/pagination';
import { useModal } from '@/logic/modal';
import { useSorting } from '@/logic/sorting';
import { ref, computed, onMounted } from 'vue';
import { useToast } from 'vue-toast-notification';
import api from '@/api/api';

const toast = useToast({ position: 'bottom-right' });

const { sortKey, sortOrder, sortBy } = useSorting();
const { showModal, isEditing, currentId, openBaseModal, closeModal } = useModal();

const admins = ref([]);
const pharmacies = ref([]);
const isLoading = ref(false);
const searchQuery = ref('');

const form = ref({ 
  firstName: '', lastName: '', email: '', phone: '', password: '', role: 2, pharmacyId: '' 
});

const fetchData = async () => {
  try {
    const [adminsRes, pharmRes] = await Promise.all([
      api.get('/Users/admins'),
      api.get('/Pharmacies')
    ]);
    admins.value = adminsRes.data;
    pharmacies.value = pharmRes.data;
  } catch (error) {
    toast.error("Ошибка загрузки данных");
  }
};

onMounted(() => { fetchData(); });

const handleResetPassword = async (id) => {
  if (!confirm('Сбросить пароль этого сотрудника до пароля по умолчанию?')) return;
  
  try {
    const response = await api.put(`/Users/${id}/reset-password`);
    toast.success("Пароль успешно сброшен!");
  } catch (error) {
    toast.error(error.response?.data?.message || "Ошибка при сбросе пароля");
  }
};

const handlePhoneInput = (event) => {
  let inputDigits = event.target.value.replace(/[^\d]/g, '');
  form.value.phone = inputDigits ? '+' + inputDigits : '';
};

const openModal = (admin = null) => {
  openBaseModal(admin?.id);

  if (admin && admin.id) {
    form.value = { 
      firstName: admin.firstName, 
      lastName: admin.lastName, 
      email: admin.email, 
      phone: admin.phone,
      password: '',
      role: admin.role,
      pharmacyId: admin.pharmacyId || ''
    };
  } else {
    form.value = { 
      firstName: '', lastName: '', email: '', phone: '', password: 'Aa1111!!', role: 2, pharmacyId: '' 
    };
  }
};

const saveAdmin = async () => {
  isLoading.value = true;
  try {
    const payload = { ...form.value };
    
    if (payload.role === 2) payload.pharmacyId = null;

    if (isEditing.value) {
      delete payload.password;
      await api.put(`/Users/edit-admin/${currentId.value}`, payload);
      toast.success("Данные сотрудника обновлены!"); 
    } else {
      await api.post('/Users/create-admin', payload);
      toast.success("Сотрудник успешно добавлен!"); 
    }
    await fetchData();
    closeModal();
  } catch (error) {
    toast.error(error.response?.data?.message || error.response?.data || "Ошибка сохранения");
  } finally {
    isLoading.value = false;
  }
};

const deleteAdmin = async (admin) => {
  if (admin.email === 'admin@unimed.pharmacy') {
    toast.error("Этого пользователя нельзя удалить!");
    return;
  }

  if (!confirm(`Вы действительно хотите удалить администратора ${admin.firstName}?`)) return;
  
  try {
    await api.delete(`/Users/delete-admin/${admin.id}`);
    await fetchData();
    toast.success("Сотрудник удален");
  } catch (error) {
    toast.error(error.response?.data?.message || "Ошибка при удалении");
  }
};

const sortedAndFilteredAdmins = computed(() => {
  let result = admins.value;

  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase();
    result = result.filter(a => 
      a.firstName.toLowerCase().includes(q) ||
      (a.lastName && a.lastName.toLowerCase().includes(q)) ||
      a.email.toLowerCase().includes(q) ||
      a.phone.includes(q)
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
  paginatedData: paginatedAdmins,
  nextPage, 
  prevPage 
} = usePagination(sortedAndFilteredAdmins, 15);
</script>

<style scoped>
.admin-users {
  padding: 20px;
}

.header-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  flex-wrap: wrap;
  gap: 15px;
}

.filters-group {
  flex: 1;
  max-width: 400px;
}

.search-input {
  width: 100%;
}

.reset-password-wrapper {
  margin-top: 10px;
  margin-bottom: 10px;
}

.reset-password-wrapper button {
  width: 100%;
}

.role-badge {
  padding: 5px 10px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 600;
}

.role-super {
  background-color: #fcf1d6;
  color: #F3C301;
}

.role-pharmacy {
  background-color: #E8F4EA;
  color: #689D6D;
}
</style>