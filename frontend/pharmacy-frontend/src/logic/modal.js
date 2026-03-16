import { ref } from 'vue';

export function useModal() {
  const showModal = ref(false);
  const isEditing = ref(false);
  const currentId = ref(null);

  const openBaseModal = (id = null) => {
    showModal.value = true;
    isEditing.value = !!id; 
    currentId.value = id || null;
  };

  const closeModal = () => {
    showModal.value = false;
  };

  return { showModal, isEditing, currentId, openBaseModal, closeModal };
}