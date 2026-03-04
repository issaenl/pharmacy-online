<template>
  <div v-if="isOpen" class="modal-overlay" @click.self="close">
    <div class="modal-content">
      <h2>Быстрое оформление</h2>
      
      <div class="item-info">
        <p class="product-name">{{ product?.name }}</p>
        <p class="pharmacy-name">{{ pharmacy?.pharmacyName }}</p>
        <p class="pharmacy-address">{{ pharmacy?.pharmacyAddress }}</p>
      </div>

      <div class="quantity-selector">
        <button @click="decrease" :disabled="quantity <= 1">-</button>
        <span>{{ quantity }}</span>
        <button @click="increase" :disabled="quantity >= maxQuantity">+</button>
      </div>

      <div class="summary">
        <span>Итого:</span>
        <span class="total-price">{{ totalPrice }} р.</span>
      </div>

      <div class="actions">
        <button class="btn-cancel" @click="close">Отмена</button>
        <button class="btn-confirm" @click="confirm" :disabled="isLoading">
          Забронировать
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';

const props = defineProps({
  isOpen: Boolean,
  product: Object,
  pharmacy: Object,
  isLoading: Boolean
});

const emit = defineEmits(['close', 'confirm']);

const quantity = ref(1);

const maxQuantity = computed(() => props.pharmacy?.quantity || 1);

const totalPrice = computed(() => {
  const price = props.pharmacy?.price || 0;
  return (price * quantity.value).toFixed(2);
});

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    quantity.value = 1;
  }
});

const decrease = () => {
  if (quantity.value > 1) quantity.value--;
};

const increase = () => {
  if (quantity.value < maxQuantity.value) quantity.value++;
};

const close = () => {
  emit('close');
};

const confirm = () => {
  emit('confirm', quantity.value);
};
</script>

<style scoped>
    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        background: rgba(0, 0, 0, 0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 10000;
    }

    .modal-content {
        background: white;
        padding: 30px;
        border-radius: 20px;
        width: 90%;
        max-width: 400px;
        display: flex;
        flex-direction: column;
        gap: 20px;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
    }

    h2 {
        margin: 0;
        font-size: 24px;
        color: #333;
        text-align: center;
        font-family: var(--main-font);
    }

    .item-info {
        display: flex;
        flex-direction: column;
        gap: 5px;
    }

    .product-name {
        font-weight: 700;
        font-size: 18px;
        margin: 0;
        color: #333;
    }

    .pharmacy-name {
        font-weight: 600;
        color: #BB4E58;
        margin: 0;
        font-size: 16px;
    }

    .pharmacy-address {
        font-size: 14px;
        color: #B4AFAC;
        margin: 0;
    }

    .quantity-selector {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 15px;
        margin: 10px 0;
    }

    .quantity-selector button {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        border: 1px solid #e0e0e0;
        background: #f9f9f9;
        font-size: 20px;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        color: #333;
        transition: all 0.2s;
    }

    .quantity-selector button:hover:not(:disabled) {
        background: #eee;
    }

    .quantity-selector button:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }

    .quantity-selector span {
        font-size: 20px;
        font-weight: 600;
        min-width: 30px;
        text-align: center;
    }

    .summary {
        display: flex;
        justify-content: space-between;
        align-items: center;
        font-size: 20px;
        font-weight: 700;
        border-top: 1px solid #eee;
        padding-top: 15px;
    }

    .total-price {
        color: #BB4E58;
    }

    .actions {
        display: flex;
        gap: 10px;
    }

    .btn-cancel, .btn-confirm {
        flex: 1;
        padding: 12px;
        border-radius: 15px;
        font-size: 16px;
        font-weight: 600;
        cursor: pointer;
        border: none;
        font-family: var(--main-font);
        transition: all 0.2s;
    }

    .btn-cancel {
        background: #f0f0f0;
        color: #333;
    }

    .btn-cancel:hover {
        background: #e4e4e4;
    }

    .btn-confirm {
        background: #BB4E58;
        color: white;
    }

    .btn-confirm:hover:not(:disabled) {
        filter: brightness(0.9);
    }

    .btn-confirm:disabled {
        opacity: 0.7;
        cursor: not-allowed;
    }
</style>