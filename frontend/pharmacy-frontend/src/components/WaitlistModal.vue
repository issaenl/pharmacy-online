<template>
  <div v-if="isOpen" class="modal-overlay" @click.stop="closeModal">
    <div class="modal-content waitlist-modal" @click.stop>
      <div class="form-group">
        <label style="font-size: 18px;">Выберите регион:</label>
        <select v-model="selectedDistrict" class="form-select" :disabled="isLoadingDistricts">
          <option v-if="isLoadingDistricts" value="">Загрузка регионов...</option>
          <option v-for="district in availableDistricts" :key="district" :value="district">
            {{ district }}
          </option>
        </select>
      </div>

      <div v-if="isAlreadyInWaitlist" class="warning-text">
        Вы уже отслеживаете этот товар в выбранном регионе.
      </div>

      <div class="modal-actions">
        <button class="btn-cancel" @click="closeModal" :disabled="isSubmitting">Отмена</button>
        <button 
          class="btn-primary" 
          @click="submit" 
          :disabled="isSubmitting || !selectedDistrict || isAlreadyInWaitlist"
          :class="{ 'btn-disabled': isAlreadyInWaitlist }">
          {{ isSubmitting ? 'Добавление...' : 'Подтвердить' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { useWaitlistStore } from '@/stores/waitlistStore';
import api from '@/api/api';

const props = defineProps({
  isOpen: Boolean,
  productId: { type: Number, required: true }
});

const emit = defineEmits(['close']);
const waitlistStore = useWaitlistStore();

const availableDistricts = ref([]);
const selectedDistrict = ref('');
const isLoadingDistricts = ref(false);
const isSubmitting = ref(false);

const isAlreadyInWaitlist = computed(() => {
  if (!selectedDistrict.value || !props.productId) return false;
  
  return waitlistStore.items.some(item => 
    item.productId === props.productId && 
    item.district.toLowerCase() === selectedDistrict.value.toLowerCase()
  );
});

onMounted(async () => {
  isLoadingDistricts.value = true;
  try {
    const res = await api.get('/Pharmacies/districts');
    availableDistricts.value = res.data;
    if (availableDistricts.value.length > 0) {
      selectedDistrict.value = availableDistricts.value[0];
    }
  } catch (error) {
    console.error("Ошибка загрузки регионов:", error);
  } finally {
    isLoadingDistricts.value = false;
  }
});

const closeModal = () => emit('close');

const submit = async () => {
  if (!props.productId || !selectedDistrict.value || isAlreadyInWaitlist.value) return;
  
  isSubmitting.value = true;
  const success = await waitlistStore.addToWaitlist(props.productId, selectedDistrict.value);
  isSubmitting.value = false;
  
  if (success) closeModal();
};
</script>

<style scoped>
    .warning-text {
      color: #BB4E58;
      font-size: 18px;
      margin-top: -15px;
      margin-bottom: 20px;
      font-weight: 600;
    }
    .btn-disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    .waitlist-modal { max-width: 450px; padding: 25px 30px; }
    .modal-desc { color: #666; font-size: 18px; margin-bottom: 20px; }
    .form-group { margin-bottom: 25px; }
</style>