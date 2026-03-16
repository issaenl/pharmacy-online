<template>
  <div class="admin-products">
      <div class="header-actions">
        <h2>Управление товарами</h2>
        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Поиск по названию..." 
          class="search-input" 
        />

        <div class="action-buttons">
          <input 
            type="file" 
            ref="fileInput" 
            @change="handleFileUpload" 
            accept=".csv" 
            style="display: none;"
          />
          <button class="btn-cancel" @click="$refs.fileInput.click()">Импорт</button>
          <button class="btn-primary" @click="openModal()">+ Добавить товар</button>
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
            <th>Фото</th>
            <th @click="sortBy('name')" class="sortable">
              Название <span v-if="sortKey === 'name'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('manufacturer')" class="sortable">
              Производитель <span v-if="sortKey === 'manufacturer'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('isPrescription')" class="sortable">
              Рецептурный <span v-if="sortKey === 'isPrescription'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('isActive')" class="sortable">
              Статус <span v-if="sortKey === 'isActive'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="product in sortedAndFilteredProducts" :key="product.id">
            <td>
              <img :src="product.pictureUrl || '/placeholder.jpg'" class="product-thumb" alt="Фото" />
            </td>
            <td>{{ product.name }}</td>
            <td>{{ product.manufacturer }}</td>
            <td class="text-center">
                <input 
                    type="checkbox" 
                    :checked="product.isPrescription" 
                    @click.prevent 
                    tabindex="-1"
                    class="readonly-checkbox"/>
            </td>
            <td>
              <span :class="['status-badge', product.isActive ? 'active' : 'inactive']">
                {{ product.isActive ? 'Активен' : 'Отключен' }}
              </span>
            </td>
            <td class="actions">
              <button class="btn-action" @click="openModal(product)" title="Редактировать">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                  <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
                </svg>
              </button>
              <button class="btn-action" @click="deleteProduct(product.id)" title="Удалить">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="3 6 5 6 21 6"></polyline>
                  <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                </svg>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <h3>{{ isEditing ? 'Редактировать товар' : 'Добавить новый товар' }}</h3>
        
        <form @submit.prevent="saveProduct" class="product-form">
          <div class="form-grid">
            <div class="form-column">
                <label>Название <input v-model="form.name" required placeholder="Магний"/></label>
                <label>Производитель <input v-model="form.manufacturer" required placeholder="Байер"/></label>
                <label>Страна <input v-model="form.country" required placeholder="Германия"/></label>
                <label>Форма выпуска <input v-model="form.dosageForm" required placeholder="таблетки"/></label>
                <label>Категория
                    <select v-model="form.categoryId" required class="form-select">
                        <option value="" disabled>Выберите категорию...</option>
                        <option v-for="cat in categories" :key="cat.id" :value="cat.id">
                        {{ cat.name }}
                        </option>
                    </select>
                </label>
                <label>Срок годности 
                    <input type="date" v-model="form.expirationDate" required />
                </label>
              
                <div class="checkboxes">
                    <label><input type="checkbox" v-model="form.isPrescription" /> Рецептурный</label>
                    <label><input type="checkbox" v-model="form.isActive" /> Активен</label>
                </div>
            </div>

            <div class="form-column files-column">
              <label>Фотография товара</label>
              <div 
                class="drop-zone" 
                @dragover.prevent="dragPhoto = true" 
                @dragleave.prevent="dragPhoto = false" 
                @drop.prevent="handleDrop($event, 'photo')"
                :class="{ 'drag-active': dragPhoto }"
                @click="$refs.photoInput.click()">
                
                <span v-if="photoFile" class="file-selected">Новое фото: {{ photoFile.name }}</span>
                <span v-else-if="form.pictureUrl" class="file-selected">Фото загружено</span>
                <span v-else>Перетащите фото сюда или кликните</span>
                
                <input type="file" ref="photoInput" class="hidden-input" @change="handleInput($event, 'photo')" accept="image/*" />
              </div>

              <label>PDF Инструкция</label>
              <div 
                class="drop-zone" 
                @dragover.prevent="dragPdf = true" 
                @dragleave.prevent="dragPdf = false" 
                @drop.prevent="handleDrop($event, 'pdf')"
                :class="{ 'drag-active': dragPdf }"
                @click="$refs.pdfInput.click()">
                
                <span v-if="pdfFile" class="file-selected">Новый PDF: {{ pdfFile.name }}</span>
                <span v-else-if="form.pdfUrl" class="file-selected">Инструкция загружена</span>
                <span v-else>Перетащите PDF сюда или кликните</span>
                
                <input type="file" ref="pdfInput" class="hidden-input" @change="handleInput($event, 'pdf')" accept="application/pdf" />
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
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useToast } from 'vue-toast-notification';
import api from '@/api/api';

const toast = useToast({ position: 'bottom-right' });

const products = ref([]);
const categories = ref([]);
const showModal = ref(false);
const isEditing = ref(false);
const isLoading = ref(false);
const currentId = ref(null);

const searchQuery = ref('');
const sortKey = ref(null); 
const sortOrder = ref(0);

const dragPhoto = ref(false);
const dragPdf = ref(false);
const photoFile = ref(null);
const pdfFile = ref(null);
const fileInput = ref(null);
const importErrors = ref([]);

const form = ref({
  name: '', manufacturer: '', country: '', dosageForm: '',
  categoryId: '', expirationDate: '', isPrescription: false, isActive: true,
  pictureUrl: null, pdfUrl: null
});

const handleFileUpload = async (event) => {
  const file = event.target.files[0];
  if (!file) return;

  const formData = new FormData();
  formData.append('file', file);

  try {
    toast.info("Загрузка файла...");
    const response = await api.post('/Products/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    
    toast.success(response.data.message);
    if (response.data.errors && response.data.errors.length > 0) {
      importErrors.value = response.data.errors;
    } else {
      importErrors.value = []; 
    }

    await fetchProducts();
  } 
  catch (error) {
    toast.error("Ошибка при импорте файла");
  } 
  finally {
    event.target.value = '';
  }
};

const sortBy = (key) => {
  if (sortKey.value === key) {
    if (sortOrder.value === 1) {
      sortOrder.value = -1;
    } 
    else if (sortOrder.value === -1) {
      sortOrder.value = 0;
      sortKey.value = null;
    }
  } 
  else {
    sortKey.value = key;
    sortOrder.value = 1;
  }
};

const sortedAndFilteredProducts = computed(() => {
  let result = products.value.filter(product => 
    product.name.toLowerCase().includes(searchQuery.value.toLowerCase())
  );

  if (sortKey.value && sortOrder.value !== 0) {
    result.sort((a, b) => {
      let valA = a[sortKey.value];
      let valB = b[sortKey.value];

      if (typeof valA === 'string') valA = valA.toLowerCase();
      if (typeof valB === 'string') valB = valB.toLowerCase();
      
      if (valA == null) valA = '';
      if (valB == null) valB = '';

      if (valA < valB) return -1 * sortOrder.value;
      if (valA > valB) return 1 * sortOrder.value;
      return 0;
    });
  }

  return result;
});

const fetchProducts = async () => {
  try {
    const response = await api.get('/Products/admin-all-products');
    products.value = response.data;
  } 
  catch (error) {
    toast.error("Ошибка загрузки товаров");
  }
};

const fetchCategories = async () => {
  try {
    const response = await api.get('/Categories');
    categories.value = response.data;
  } 
  catch (error) {
    toast.error("Ошибка загрузки категорий");
  }
};

onMounted(() => {
  fetchProducts();
  fetchCategories();
});

const openModal = (product = null) => {
  showModal.value = true;
  photoFile.value = null;
  pdfFile.value = null;

  if (product) {
    isEditing.value = true;
    currentId.value = product.id;

    form.value = {
      ...product,
      expirationDate: product.expirationDate ? product.expirationDate.split('T')[0] : ''
    };
  } else {
    isEditing.value = false;
    currentId.value = null;
    form.value = { name: '', manufacturer: '', country: '', dosageForm: '', categoryId: '', expirationDate: '', isPrescription: false, isActive: true, pictureUrl: null, pdfUrl: null };
  }
};

const closeModal = () => { showModal.value = false; };

const handleDrop = (e, type) => {
  const file = e.dataTransfer.files[0];
  assignFile(file, type);
  type === 'photo' ? dragPhoto.value = false : dragPdf.value = false;
};

const handleInput = (e, type) => {
  const file = e.target.files[0];
  assignFile(file, type);
};

const assignFile = (file, type) => {
  if (!file) return;
  if (type === 'photo' && file.type.startsWith('image/')) photoFile.value = file;
  if (type === 'pdf' && file.type === 'application/pdf') pdfFile.value = file;
};

const saveProduct = async () => {
  isLoading.value = true;
  try {
    const formData = new FormData();
    
    formData.append('Name', form.value.name);
    formData.append('Manufacturer', form.value.manufacturer);
    formData.append('Country', form.value.country);
    formData.append('DosageForm', form.value.dosageForm);
    formData.append('CategoryId', form.value.categoryId);
    formData.append('IsPrescription', form.value.isPrescription);
    formData.append('IsActive', form.value.isActive);

    if (form.value.expirationDate) {
      formData.append('ExpirationDate', form.value.expirationDate);
    }
    
    if (photoFile.value) 
    {
        formData.append('PictureFile', photoFile.value);
    }
    if (pdfFile.value) 
    {
        formData.append('PdfFile', pdfFile.value);
    }

    const axiosConfig = {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    };

    if (isEditing.value) {
      await api.put(`/Products/${currentId.value}`, formData, axiosConfig);
    } 
    else {
      await api.post('/Products', formData, axiosConfig);
    }
    
    await fetchProducts();
    closeModal();
    toast.success("Товар успешно сохранен!"); 
  } 
  catch (error) {
    let errorMsg = error.message;
    if (error.response?.data?.errors) {
      errorMsg = Object.values(error.response.data.errors).flat().join('\n');
    }
    toast.error("Ошибка при сохранении:\n" + errorMsg);
  } 
  finally {
    isLoading.value = false;
  }
};

const deleteProduct = async (id) => {
  if (!confirm('Вы уверены, что хотите удалить этот товар?')) 
  {
    return;
  }
  try {
    await api.delete(`/Products/${id}`);
    await fetchProducts();
    toast.success("Товар успешно удален");
  } 
  catch (error) {
    toast.error("Ошибка удаления");
  }
};
</script>

<style scoped>
.admin-products {
  padding: 20px;
}

.readonly-checkbox {
  width: 18px;
  height: 18px;
  cursor: default;
  accent-color: var(--primary-color);
}

.product-thumb {
  width: 50px;
  height: 50px;
  object-fit: cover;
  border-radius: 8px;
}

.status-badge {
  padding: 5px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: bold;
}

.status-badge.active {
  background: #E8F4EA;
  color: #689D6D;
  font-size: 14px;
}

.status-badge.inactive {
  background: #FDE8E8;
  color: #BB4E58;
  font-size: 14px;
}

.checkboxes {
  display: flex;
  gap: 20px;
  margin-top: 10px;
}

.checkboxes label {
  flex-direction: row;
  align-items: center;
  font-weight: normal;
}

.files-column {
  justify-content: flex-start;
}

.drop-zone {
  border: 2px dashed var(--primary-light);
  border-radius: 12px;
  padding: 30px;
  text-align: center;
  cursor: pointer;
  transition: 0.3s;
  background: var(--background-color);
  color: #666;
  font-size: 16px;
}

.drop-zone.drag-active {
  background: var(--primary-light);
  border-color: var(--primary-color);
  color: white;
}

.hidden-input {
  display: none;
}

.file-selected {
  color: var(--primary-color);
  font-weight: bold;
}

.error-list::-webkit-scrollbar {
  width: 6px;
}

.error-list::-webkit-scrollbar-track {
  background: transparent;
}

.error-list::-webkit-scrollbar-thumb {
  background-color: #fca5a5;
  border-radius: 10px;
}
</style>