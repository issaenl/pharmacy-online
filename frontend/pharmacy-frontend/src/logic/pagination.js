import { ref, computed, watch } from 'vue';

export function usePagination(dataArray, itemsPerPage = 10) {
  const currentPage = ref(1);

  watch(dataArray, () => {
    currentPage.value = 1;
  });

  const totalPages = computed(() => {
    return Math.ceil(dataArray.value.length / itemsPerPage) || 1;
  });

  const paginatedData = computed(() => {
    const start = (currentPage.value - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    return dataArray.value.slice(start, end);
  });

  const nextPage = () => {
    if (currentPage.value < totalPages.value) currentPage.value++;
  };

  const prevPage = () => {
    if (currentPage.value > 1) currentPage.value--;
  };

  return {
    currentPage,
    totalPages,
    paginatedData,
    nextPage,
    prevPage
  };
}