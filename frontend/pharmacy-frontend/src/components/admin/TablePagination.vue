<template>
  <div class="pagination-wrapper" v-if="totalPages > 1">
    <button 
      class="btn-page" 
      :disabled="currentPage === 1" 
      @click="$emit('prev')">
      &larr; Назад
    </button>
    
    <span class="page-info">
      Страница <strong>{{ currentPage }}</strong> из {{ totalPages }}
    </span>
    
    <button 
      class="btn-page" 
      :disabled="currentPage === totalPages" 
      @click="$emit('next')"
    >
      Вперед &rarr;
    </button>
  </div>
</template>

<script setup>
defineProps({
  currentPage: Number,
  totalPages: Number
});
defineEmits(['prev', 'next']);
</script>

<style scoped>
.pagination-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 15px;
  padding: 15px 0;
  margin-top: 10px;
}

.btn-page {
  padding: 8px 16px;
  border: none;
  background: #689d6d;
  color: white;
  border-radius: 8px;
  cursor: pointer;
  transition: 0.2s;
  font-weight: 500;
  font-family: var(--main-font);
  font-size: 16px;
}

.btn-page:hover:not(:disabled) {
  background: var(--primary-light);
  border-color: var(--primary-color, #689d6d);
}

.btn-page:disabled {
  background: #f1f5f9;
  color: #94a3b8;
  cursor: not-allowed;
}

.page-info {
  font-size: 16px;
  color: #64748b;
}
</style>