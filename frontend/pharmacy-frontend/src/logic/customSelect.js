import { ref, computed, onMounted, onUnmounted } from 'vue';

export function useCustomSelect(itemsArray, searchField = 'name') {
  const showDropdown = ref(false);
  const searchQuery = ref('');

  const filteredItems = computed(() => {
    if (!searchQuery.value) return itemsArray.value;
    const q = searchQuery.value.toLowerCase();
    
    return itemsArray.value.filter(item => {
      const fieldValue = item[searchField];
      return fieldValue && fieldValue.toLowerCase().includes(q);
    });
  });

  const toggleDropdown = () => {
    showDropdown.value = !showDropdown.value;
    if (showDropdown.value) {
      searchQuery.value = '';
    }
  };

  const closeDropdown = () => {
    showDropdown.value = false;
  };

  onMounted(() => {
    document.addEventListener('click', closeDropdown);
  });

  onUnmounted(() => {
    document.removeEventListener('click', closeDropdown);
  });

  return {
    showDropdown,
    searchQuery,
    filteredItems,
    toggleDropdown,
    closeDropdown
  };
}