<template>
  <div class="pagination" v-if="totalPages > 1">
    <button 
      class="page-arrow" 
      @click="changePage(currentPage - 1)" 
      :disabled="currentPage === 1"
    >
      ‹
    </button>
    
    <button 
      v-for="page in totalPages" 
      :key="page" 
      :class="['page-num', { active: currentPage === page }]" 
      @click="changePage(page)"
    >
      {{ page }}
    </button>
    
    <button 
      class="page-arrow" 
      @click="changePage(currentPage + 1)" 
      :disabled="currentPage === totalPages"
    >
      ›
    </button>
  </div>
</template>

<script setup>
const props = defineProps({
  currentPage: {
    type: Number,
    required: true
  },
  totalPages: {
    type: Number,
    required: true
  }
});

const emit = defineEmits(['update:currentPage']);

const changePage = (page) => {
  if (page >= 1 && page <= props.totalPages) {
    emit('update:currentPage', page);
  }
};
</script>

<style scoped>
.pagination { 
  display: flex; 
  justify-content: center; 
  align-items: center; 
  gap: 12px; 
  margin: 40px 0; 
}

.page-num, .page-arrow { 
  width: 44px; 
  height: 44px; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  border-radius: 50%; 
  border: none; 
  cursor: pointer; 
  font-size: 20px; 
  font-weight: 600; 
  transition: all 0.2s ease; 
  background: #DCDCDC;
  color: #333; 
  font-family: var(--main-font);
}

.page-arrow { 
  color: #888; 
  background: #DCDCDC;
}

.page-arrow:not(:disabled):hover { 
  background: var(--primary-light);
  color: white; 
}

.page-arrow:disabled { 
  cursor: not-allowed; 
  opacity: 0.4; 
}

.page-num.active { 
  background: var(--primary-color, #689D6D); 
  color: #fff; 
}

.page-num:not(.active):hover { 
  background: var(--primary-light);
  color: white; 
}

@media (max-width: 600px) {
  .pagination { 
    gap: 6px; 
    flex-wrap: wrap; 
  }
  .page-num, .page-arrow { 
    width: 38px; 
    height: 38px; 
    font-size: 14px; 
  }
}
</style>