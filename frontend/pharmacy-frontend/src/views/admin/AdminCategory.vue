<template>
  <div class="admin-categories">
    <div class="header-actions">
      <h2>Управление категориями</h2>
      <input 
        type="text" 
        v-model="searchQuery" 
        placeholder="Поиск по названию или тегам..." 
        class="search-input"/>
      <div class="action-buttons">
        <input 
          type="file" 
          ref="fileInput" 
          @change="handleFileUpload" 
          accept=".csv, .xlsx, .xls" 
          style="display: none;"/>
        <button class="btn-cancel" @click="$refs.fileInput.click()">Импорт</button>
        <button class="btn-primary" @click="openModal()">+ Добавить категорию</button>
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
            <th @click="sortBy('name')" class="sortable">
              Название <span v-if="sortKey === 'name'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Описание</th>
            <th>Теги (симптомы / поиск)</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="category in sortedAndFilteredCategories" :key="category.id">
            <td><strong>{{ category.name }}</strong></td>
            <td class="text-muted">{{ category.description || '—' }}</td>
            <td>
              <div class="tags-container">
                <span class="tag-badge" v-for="tag in getVisibleTags(category)" :key="tag">
                  {{ tag }}
                </span>
                
                <button 
                  v-if="category.tags && category.tags.length > 3" 
                  class="btn-toggle-tags"
                  @click="toggleTags(category.id)"
                >
                  {{ expandedRows.includes(category.id) ? 'Скрыть' : `+${category.tags.length - 3} ещё` }}
                </button>
                <span v-if="!category.tags || category.tags.length === 0" class="text-muted">Нет тегов</span>
              </div>
            </td>
            <td class="actions">
              <button class="btn-action" @click="openModal(category)" title="Редактировать">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
              </button>
              <button class="btn-action" @click="deleteCategory(category.id)" title="Удалить">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
              </button>
            </td>
          </tr>
          <tr v-if="sortedAndFilteredCategories.length === 0">
            <td colspan="4" class="text-center text-muted" style="padding: 30px;">Категории не найдены</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <h3>{{ isEditing ? 'Редактировать категорию' : 'Новая категория' }}</h3>
        
        <form @submit.prevent="saveCategory" class="pharmacy-form">
          <div class="form-column">
              <label>Название
                <input type="text" v-model="form.name" required placeholder="Жаропонижающие" />
              </label>
              
              <label>Описание
                <textarea v-model="form.description" rows="3" placeholder="Краткое описание категории..."></textarea>
              </label>

              <div class="tags-editor-section">
                <label>Теги для поиска (симптомы)</label>
                <div class="info-box">
                  По этим словам покупатели будут находить товары. Введите слово и нажмите <strong>Enter</strong> или кнопку <strong>Добавить</strong>.
                </div>
                
                <div class="tag-input-wrapper">
                  <input 
                    type="text" 
                    v-model="currentTag" 
                    @keydown.enter.prevent="addTag" 
                    placeholder="Например: мигрень, жар..." 
                  />
                  <button type="button" class="btn-add-tag" @click="addTag">Добавить</button>
                </div>

                <div class="tags-preview" v-if="form.tags.length > 0">
                  <span class="tag-badge removable" v-for="(tag, index) in form.tags" :key="index">
                    {{ tag }}
                    <button type="button" class="remove-tag-btn" @click="removeTag(index)">✕</button>
                  </span>
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

const categories = ref([]);
const showModal = ref(false);
const isLoading = ref(false);
const isEditing = ref(false);
const currentId = ref(null);

const searchQuery = ref('');
const sortKey = ref(null); 
const sortOrder = ref(0);

const form = ref({ name: '', description: '', tags: [] });
const currentTag = ref('');
const expandedRows = ref([]);

const fileInput = ref(null);
const importErrors = ref([]);

const handleFileUpload = async (event) => {
  const file = event.target.files[0];
  if (!file) return;

  const formData = new FormData();
  formData.append('file', file);

  try {
    toast.info("Загрузка файла...");
    const response = await api.post('/Categories/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    
    toast.success(response.data.message);
    if (response.data.errors?.length > 0) {
      importErrors.value = response.data.errors;
    } else {
      importErrors.value = [];
    }
    await fetchCategories();
  } catch (error) {
    toast.error(error.response?.data?.message || "Ошибка импорта");
  } finally {
    event.target.value = '';
  }
};

const fetchCategories = async () => {
  try {
    const response = await api.get('/Categories');
    categories.value = response.data;
  } catch (error) {
    toast.error("Ошибка загрузки категорий");
  }
};

onMounted(() => { fetchCategories(); });

const addTag = () => {
  const tag = currentTag.value.trim().toLowerCase();
  if (tag && !form.value.tags.includes(tag)) {
    form.value.tags.push(tag);
  } else if (form.value.tags.includes(tag)) {
    toast.info("Такой тег уже добавлен");
  }
  currentTag.value = '';
};

const removeTag = (index) => {
  form.value.tags.splice(index, 1);
};

const toggleTags = (categoryId) => {
  if (expandedRows.value.includes(categoryId)) {
    expandedRows.value = expandedRows.value.filter(id => id !== categoryId);
  } else {
    expandedRows.value.push(categoryId);
  }
};

const getVisibleTags = (category) => {
  if (!category.tags) return [];
  if (expandedRows.value.includes(category.id)) {
    return category.tags;
  }
  return category.tags.slice(0, 3);
};

const openModal = (category = null) => {
  showModal.value = true;
  currentTag.value = '';
  
  if (category && category.id) {
    isEditing.value = true;
    currentId.value = category.id;
    form.value = { 
      name: category.name, 
      description: category.description, 
      tags: [...(category.tags || [])] 
    };
  } else {
    isEditing.value = false;
    currentId.value = null;
    form.value = { name: '', description: '', tags: [] };
  }
};

const closeModal = () => { showModal.value = false; };

const saveCategory = async () => {
  if (currentTag.value.trim() !== '') {
    addTag();
  }

  isLoading.value = true;
  try {
    const payload = {
      name: form.value.name,
      description: form.value.description,
      tags: form.value.tags
    };

    const axiosConfig = { headers: { 'Content-Type': 'application/json' } };

    if (isEditing.value) {
      await api.put(`/Categories/${currentId.value}`, payload, axiosConfig);
      toast.success("Категория обновлена!"); 
    } else {
      await api.post('/Categories', payload, axiosConfig);
      toast.success("Категория добавлена!"); 
    }
    await fetchCategories();
    closeModal();
  } catch (error) {
    toast.error(error.response?.data?.message || "Ошибка сохранения");
  } finally {
    isLoading.value = false;
  }
};

const deleteCategory = async (id) => {
  if (!confirm('Удалить эту категорию?')) return;
  try {
    const response = await api.delete(`/Categories/${id}`);
    await fetchCategories();
    toast.success(response.data?.message || "Удалено");
  } catch (error) {
    toast.error(error.response?.data?.message || "Ошибка удаления");
  }
};

const sortBy = (key) => {
  if (sortKey.value === key) {
    if (sortOrder.value === 1) sortOrder.value = -1;
    else if (sortOrder.value === -1) { sortOrder.value = 0; sortKey.value = null; }
  } else { sortKey.value = key; sortOrder.value = 1; }
};

const sortedAndFilteredCategories = computed(() => {
  let result = categories.value;

  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase();
    result = result.filter(c => 
      c.name.toLowerCase().includes(q) ||
      (c.tags && c.tags.some(t => t.toLowerCase().includes(q)))
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
.admin-categories {
  padding: 20px;
}

.modal-content {
  max-width: 600px;
}

.form-column label {
  font-size: 16px;
}

.form-column input,
.form-column textarea {
  font-size: 15px;
}

.form-column textarea {
  resize: vertical;
}

.info-box {
  padding: 10px;
  font-size: 14px;
  margin-bottom: 5px;
}

.tags-container {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  align-items: center;
}

.tag-badge {
  background: #e8f4ea;
  color: #689d6d;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 13px;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
}

.btn-toggle-tags {
  background: none;
  border: 1px dashed #689d6d;
  color: #689d6d;
  border-radius: 12px;
  padding: 3px 8px;
  font-size: 12px;
  cursor: pointer;
  transition: 0.2s;
  font-weight: 600;
}

.btn-toggle-tags:hover {
  background: #689d6d;
  color: white;
}

.tags-editor-section {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-top: 10px;
}

.tags-editor-section label {
  margin-bottom: 0;
}

.tag-input-wrapper {
  display: flex;
  gap: 10px;
}

.tag-input-wrapper input {
  flex: 1;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-family: inherit;
  font-size: 15px;
  font-weight: normal;
}

.tag-input-wrapper input:focus {
  border-color: var(--primary-color);
  outline: none;
}

.btn-add-tag {
  background: #689d6d;
  border: 1px solid #cbd5e1;
  color: white;
  border-radius: 8px;
  padding: 0 20px;
  font-weight: bold;
  font-family: var(--main-font);
  font-size: 16px;
  cursor: pointer;
  transition: 0.2s;
}

.btn-add-tag:hover {
  background: #e2e8f0;
}

.tags-preview {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 10px;
  padding: 15px;
  background: #fafafa;
  border-radius: 8px;
  border: 2px dashed #ddd;
  min-height: 50px;
}

.tag-badge.removable {
  background: white;
  border: 1px solid #689d6d;
  padding-right: 4px;
}

.remove-tag-btn {
  background: none;
  border: none;
  color: #689d6d;
  font-size: 16px;
  margin-left: 5px;
  cursor: pointer;
  padding: 0 4px;
  font-weight: bold;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  width: 20px;
  height: 20px;
  transition: 0.2s;
}

.remove-tag-btn:hover {
  background: #fee2e2;
}
</style>