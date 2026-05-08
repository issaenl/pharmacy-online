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

    <ErrorAlert :errors="importErrors" @clear="importErrors = []" />

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
          <tr v-for="product in paginatedProducts" :key="product.id">
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
            <TableActions 
                @edit="openModal(product)" 
                @delete="deleteProduct(product.id)" />
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
      :title="isEditing ? 'Редактировать товар' : 'Новый товар'"
      @close="closeModal">
        <form @submit.prevent="saveProduct" class="product-form">
          <div class="form-grid">
            <div class="form-column">
                <label>Название <input v-model="form.name" required placeholder="Магний"/></label>
                <label>Производитель <input v-model="form.manufacturer" required placeholder="Байер"/></label>
                <label>Страна <input v-model="form.country" required placeholder="Германия"/></label>
                <label>Форма выпуска <input v-model="form.dosageForm" required placeholder="таблетки"/></label>
                <label>Категория
                  <div class="custom-select-wrapper" @click.stop>
                    <div class="selected-option" @click="toggleCategoryDropdown">
                      {{ selectedCategoryName }}
                      <span class="arrow" :class="{ 'arrow-up': showCategoryDropdown }">▼</span>
                    </div>

                   <div v-if="showCategoryDropdown" class="dropdown-menu">
                      <ul class="options-list">
                        <li
                          v-for="cat in filteredCategories"
                          :key="cat.id"
                          @click="selectCategory(cat)"
                          :class="{ selected: form.categoryId === cat.id }"
                        >
                          {{ cat.name }}
                        </li>
                        <li v-if="filteredCategories.length === 0" class="no-options">
                          Категория не найдена
                        </li>
                      </ul>
                      
                      <input
                        type="text"
                        v-model="categorySearch"
                        placeholder="Поиск категории..."
                        class="search-box"
                        @click.stop
                      />
                    </div>
                  </div>
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
                
                <span v-if="photoFile" class="file-selected">Новое фото: {{ photoFile.name }} <button type="button" class="btn-clear" @click.stop="photoFile = null">✕</button></span>

                <div v-else-if="form.pictureUrl" class="file-actions">
                  <span class="file-selected">Фото загружено</span>
                  <button type="button" class="btn-delete-cloud" @click.stop="removeExistingFile('photo')" title="Удалить из облака">Удалить ✕</button>
                </div>
                
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
                
                <span v-if="pdfFile" class="file-selected">Новый PDF: {{ pdfFile.name }} <button type="button" class="btn-clear" @click.stop="pdfFile = null">✕</button></span>
                <div v-else-if="form.pdfUrl" class="file-actions">
                  <span class="file-selected">Инструкция загружена</span>
                  <button type="button" class="btn-delete-cloud" @click.stop="removeExistingFile('pdf')" title="Удалить из облака">Удалить ✕</button>
                </div>
                
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
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useCustomSelect } from '@/logic/customSelect';
import { useToast } from 'vue-toast-notification';
import api from '@/api/api';

const toast = useToast({ position: 'bottom-right' });

const { sortKey, sortOrder, sortBy } = useSorting();
const { showModal, isEditing, currentId, openBaseModal, closeModal } = useModal();

const products = ref([]);
const categories = ref([]);
const isLoading = ref(false);
const searchQuery = ref('');
const dragPhoto = ref(false);
const dragPdf = ref(false);
const photoFile = ref(null);
const pdfFile = ref(null);
const fileInput = ref(null);
const importErrors = ref([]);

const { 
  showDropdown: showCategoryDropdown, 
  searchQuery: categorySearch, 
  filteredItems: filteredCategories, 
  toggleDropdown: toggleCategoryDropdown,
  closeDropdown: closeCategoryDropdown
} = useCustomSelect(categories, 'name');

const selectedCategoryName = computed(() => {
  if (!form.value.categoryId) return 'Выберите категорию...';
  const cat = categories.value.find(c => c.id === form.value.categoryId);
  return cat ? cat.name : 'Выберите категорию...';
});

const selectCategory = (cat) => {
  form.value.categoryId = cat.id;
  closeCategoryDropdown();
};

const form = ref({
  name: '', manufacturer: '', country: '', dosageForm: '',
  categoryId: '', isPrescription: false, isActive: true,
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
    importErrors.value = error.response.data.errors;
    toast.error("Ошибка при импорте файла");
  } 
  finally {
    event.target.value = '';
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

const { 
  currentPage, 
  totalPages, 
  paginatedData: paginatedProducts,
  nextPage, 
  prevPage 
} = usePagination(sortedAndFilteredProducts, 15);

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
  openBaseModal(product?.id);
  photoFile.value = null;
  pdfFile.value = null;
  if (product && product.id) {
    form.value = {
      ...product
    };
  } else {
    form.value = { 
      name: '', 
      manufacturer: '', 
      country: '', 
      dosageForm: '', 
      categoryId: '', 
      isPrescription: false, 
      isActive: true, 
      pictureUrl: null, 
      pdfUrl: null 
    };
  }
};

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

const removeExistingFile = async (fileType) => {
  if (!isEditing.value) {
    if (fileType === 'photo') form.value.pictureUrl = null;
    if (fileType === 'pdf') form.value.pdfUrl = null;
    return;
  }

  const fileLabel = fileType === 'photo' ? 'фотографию' : 'инструкцию';
  if (!confirm(`Вы уверены, что хотите безвозвратно удалить ${fileLabel} из облака?`)) {
    return;
  }

  isLoading.value = true;
  try {
    const endpoint = `/Products/${currentId.value}/${fileType}`;
    await api.delete(endpoint);
    
    if (fileType === 'photo') form.value.pictureUrl = null;
    if (fileType === 'pdf') form.value.pdfUrl = null;
    fetchProducts();
    toast.success(`${fileLabel === 'фотографию' ? 'Фото удалено' : 'Инструкция удалена'} из облака!`);
  } catch (error) {
    toast.error(`Ошибка при удалении ${fileLabel}`);
    console.error(error);
  } finally {
    isLoading.value = false;
  }
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

.file-actions {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}

.btn-delete-cloud {
  background: #BB4E58;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: bold;
  font-family: var(--main-font);
  transition: 0.2s;
}

.btn-delete-cloud:hover {
  background: #9a3d46;
}

.btn-clear {
  background: transparent;
  color: #BB4E58;
  border: none;
  cursor: pointer;
  font-weight: bold;
  font-family: var(--main-font);
  margin-left: 5px;
}

.btn-clear:hover {
  color: #9a3d46;
}
</style>