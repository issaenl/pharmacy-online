import { ref } from 'vue';

export function useSorting() {
  const sortKey = ref(null);
  const sortOrder = ref(0);

  const sortBy = (key) => {
    if (sortKey.value === key) {
      if (sortOrder.value === 1) sortOrder.value = -1;
      else if (sortOrder.value === -1) { 
        sortOrder.value = 0; 
        sortKey.value = null; 
      }
    } else { 
      sortKey.value = key; 
      sortOrder.value = 1; 
    }
  };

  return { sortKey, sortOrder, sortBy };
}