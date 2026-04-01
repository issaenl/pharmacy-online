<template>
  <div class="admin-reviews">
    <div class="header-actions">
      <h2>Модерация отзывов</h2>
      
      <div class="filters-group">
        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Поиск по аптеке, имени или тексту..." 
          class="search-input" 
        />
      </div>
    </div>

    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th @click="sortBy('createdAt')" class="sortable">
              Дата <span v-if="sortKey === 'createdAt'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('userName')" class="sortable">
              Покупатель <span v-if="sortKey === 'userName'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th @click="sortBy('pharmacyName')" class="sortable">
              Аптека <span v-if="sortKey === 'pharmacyName'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Заказ</th>
            <th @click="sortBy('rating')" class="sortable text-center">
              Оценка <span v-if="sortKey === 'rating'" class="sort-icon">{{ sortOrder === 1 ? '▲' : '▼' }}</span>
            </th>
            <th>Комментарий</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="review in paginatedReviews" :key="review.id">
            <td class="text-muted">{{ formatDate(review.createdAt) }}</td>
            <td><strong>{{ review.userName }}</strong></td>
            <td>{{ review.pharmacyName }}</td>
            <td><span class="order-badge">№{{ review.orderId }}</span></td>
            <td class="text-center">
              <RatingBadge :rating="review.rating" />
            </td>
            <td class="comment-cell">
              <div class="comment-text" :title="review.comment">{{ review.comment || '—' }}</div>
            </td>
            
            <td class="actions-cell">
              <button 
                class="icon-action-btn hover-approve" 
                @click="moderateReview(review.id, 1)"
                title="Одобрить и опубликовать"
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>
              </button>

              <button 
                class="icon-action-btn hover-reject" 
                @click="moderateReview(review.id, 2)"
                title="Отклонить (скрыть)"
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
              </button>

              <button 
                class="icon-action-btn hover-ban" 
                @click="banUser(review.userId, review.userName, review.id)"
                title="Заблокировать пользователя"
              >
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="4.93" y1="4.93" x2="19.07" y2="19.07"></line></svg>
              </button>
            </td>
          </tr>
          
          <tr v-if="!isLoading && sortedAndFilteredReviews.length === 0">
            <td colspan="7" class="text-center text-muted empty-state">
              <br>
              Все отзывы проверены.
            </td>
          </tr>
          <tr v-if="isLoading">
            <td colspan="7" class="text-center text-muted" style="padding: 40px;">
              Загрузка...
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <TablePagination 
      v-if="sortedAndFilteredReviews.length > 0"
      :current-page="currentPage" 
      :total-pages="totalPages" 
      @prev="prevPage" 
      @next="nextPage" 
    />

    <Modal 
      :show="isRejectModalOpen" 
      title="Отклонение отзыва" 
      @close="closeRejectModal"
    >
      <div class="reject-form">
        <p class="reject-hint">Укажите причину. Покупатель увидит её в личном кабинете и сможет исправить свой отзыв.</p>
        
        <textarea 
          v-model="rejectReasonText" 
          rows="4" 
          placeholder="Например: Отзыв содержит нецензурную лексику или не относится к заказу..."
          class="reject-textarea"
        ></textarea>
        
        <div class="modal-actions">
          <button class="btn-cancel" @click="closeRejectModal">Отмена</button>
          <button 
            class="btn-submit-reject" 
            :disabled="!rejectReasonText.trim() || isSubmitting"
            @click="submitReject"
          >
            {{ isSubmitting ? 'Сохранение...' : 'Отклонить отзыв' }}
          </button>
        </div>
      </div>
    </Modal>
  </div>
</template>

<script setup>
import TablePagination from '@/components/admin/TablePagination.vue';
import Modal from '@/components/admin/Modal.vue';
import RatingBadge from '@/components/RatingBadge.vue';
import { usePagination } from '@/logic/pagination';
import { useSorting } from '@/logic/sorting';
import { ref, computed, onMounted } from 'vue';
import { useToast } from 'vue-toast-notification';
import api from '@/api/api';

const toast = useToast({ position: 'bottom-right' });
const { sortKey, sortOrder, sortBy } = useSorting();

const reviews = ref([]);
const searchQuery = ref('');
const isLoading = ref(true);

const isRejectModalOpen = ref(false);
const currentRejectReviewId = ref(null);
const rejectReasonText = ref('');
const isSubmitting = ref(false);

const fetchPendingReviews = async () => {
  isLoading.value = true;
  try {
    const response = await api.get('/Reviews/pending');
    reviews.value = response.data;
  } catch (error) {
    toast.error("Ошибка загрузки списка отзывов");
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => { fetchPendingReviews(); });

const banUser = async (userId, userName, reviewId) => {
  if (!userId) {
    toast.error("Ошибка: ID пользователя неизвестен.");
    return;
  }

  if (!confirm(`Вы уверены, что хотите заблокировать покупателя ${userName}? Его аккаунт будет заблокирован, а текущий отзыв автоматически отклонен.`)) {
    return;
  }

  try {
    const banResponse = await api.put(`/Users/ban/${userId}`);
    toast.success(banResponse.data.message || `Пользователь ${userName} заблокирован`);
    if (reviewId) {
      const reason = encodeURIComponent("Aккаунт заблокирован за нарушение правил сервиса.");
      await api.put(`/Reviews/${reviewId}/moderate?newStatus=2&rejectReason=${reason}`);
      toast.success("Отзыв автоматически отклонен");
    }
    await fetchPendingReviews();
    
  } catch (error) {
    console.error(error);
    toast.error("Ошибка при блокировке пользователя или отклонении отзыва");
  }
};

const openRejectModal = (id) => {
  currentRejectReviewId.value = id;
  rejectReasonText.value = '';
  isRejectModalOpen.value = true;
};

const closeRejectModal = () => {
  isRejectModalOpen.value = false;
  currentRejectReviewId.value = null;
  rejectReasonText.value = '';
};

const submitReject = async () => {
  if (!rejectReasonText.value.trim() || !currentRejectReviewId.value) return;

  isSubmitting.value = true;
  try {
    const url = `/Reviews/${currentRejectReviewId.value}/moderate?newStatus=2&rejectReason=${encodeURIComponent(rejectReasonText.value.trim())}`;
    const response = await api.put(url);
    toast.success(response.data.message || "Отзыв успешно отклонен");
    closeRejectModal();
    await fetchPendingReviews();
  } catch (error) {
    toast.error("Ошибка при модерации отзыва");
  } finally {
    isSubmitting.value = false;
  }
};

const moderateReview = async (id, status) => {
  if (status === 2) {
    openRejectModal(id);
    return;
  }

  if (!confirm('Вы уверены, что хотите одобрить и опубликовать этот отзыв?')) return;

  try {
    const response = await api.put(`/Reviews/${id}/moderate?newStatus=${status}`);
    toast.success(response.data.message || "Статус отзыва обновлен");
    await fetchPendingReviews();
  } catch (error) {
    toast.error("Ошибка при модерации отзыва");
  }
};

const formatDate = (dateString) => {
  const date = new Date(dateString);
  return date.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
};

const sortedAndFilteredReviews = computed(() => {
  let result = reviews.value;

  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase();
    result = result.filter(r => 
      r.pharmacyName.toLowerCase().includes(q) ||
      r.userName.toLowerCase().includes(q) ||
      (r.comment && r.comment.toLowerCase().includes(q)) ||
      r.orderId.toString().includes(q)
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
  paginatedData: paginatedReviews,
  nextPage, 
  prevPage 
} = usePagination(sortedAndFilteredReviews, 15);
</script>

<style scoped>
.admin-reviews { padding: 20px; }

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

.order-badge {
  color: #333;
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
}

.comment-cell {
  max-width: 250px;
}

.comment-text {
  font-style: italic;
  color: #555;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  cursor: help;
}

.actions-cell {
  display: flex;
  gap: 8px;
  justify-content: flex-start;
  align-items: center;
}

.icon-action-btn {
  background: none;
  border: none;
  padding: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #A0A0A0; 
  transition: color 0.2s ease, transform 0.1s ease;
  border-radius: 50%;
}

.icon-action-btn:hover {
  transform: scale(1.1); 
  background: #f9f9f9;
}

.icon-action-btn.hover-approve:hover { color: #689D6D; background: #E8F4EA;} 
.icon-action-btn.hover-reject:hover { color: #BB4E58; background: #FDE8E8;} 
.icon-action-btn.hover-ban:hover { color: #333; background: #E5E7EB;} 

.empty-state {
  padding: 60px !important;
  font-size: 16px;
}

.reject-form {
  display: flex;
  flex-direction: column;
  gap: 15px;
  padding-top: 10px;
}

.reject-hint {
  font-size: 14px;
  color: #666;
  margin: 0;
}

.reject-textarea {
  width: 100%;
  padding: 12px;
  border-radius: 10px;
  border: 1px solid #ddd;
  font-family: inherit;
  font-size: 15px;
  resize: vertical;
  outline: none;
  transition: border-color 0.2s;
}

.reject-textarea:focus {
  border-color: #BB4E58;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 10px;
}

.btn-cancel, .btn-submit-reject {
  padding: 10px 20px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 15px;
  cursor: pointer;
  font-family: inherit;
  transition: 0.2s;
}

.btn-cancel {
  background: transparent;
  border: 1px solid #ccc;
  color: #555;
}

.btn-cancel:hover {
  background: #f5f5f5;
}

.btn-submit-reject {
  background: #BB4E58;
  color: white;
  border: none;
}

.btn-submit-reject:hover:not(:disabled) {
  background: #a33f48;
}

.btn-submit-reject:disabled {
  background: #e5b0b5;
  cursor: not-allowed;
}
</style>