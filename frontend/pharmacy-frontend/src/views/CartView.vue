<template>
  <TheHeader />
  <div class="cart-page">
    <div class="container cart-layout">
      
      <div class="cart-items-section">
        <div class="cart-header">
          <div>
            <h2>Корзина</h2>
          </div>
          <button @click="cartStore.clearCart" class="clear-btn" v-if="cartStore.items.length > 0">Очистить корзину</button>
        </div>

        <div v-if="cartStore.items.length === 0" class="empty-cart">
          <p>Ваша корзина пуста</p>
          <router-link to="/full-catalog" class="go-catalog-btn">В каталог</router-link>
        </div>

        <div class="cart-list" v-else>
          <CartItem 
            v-for="item in cartStore.items" 
            :key="item.productId" 
            :item="item" 
          />
        </div>
      </div>

      <aside class="cart-summary">
        <h2>Ваш заказ</h2>

        <div class="summary-row">
          <span style="font-size: 16px;">В заказе товаров: {{ cartStore.totalItems }}</span>
        </div>
        
        <div class="summary-total">
          <span class="total-label">Общая стоимость</span>
        </div>
        
        <div class="summary-row total-price-row">
          <span class="total-price">от {{ cartStore.totalPrice.toFixed(2) }} р.</span>
        </div>

        <div class="pharmacy-selection">
          <button 
            v-if="!cartStore.selectedPharmacy" 
            class="checkout-btn select-map-btn" 
            @click="isMapOpen = true"
            :disabled="cartStore.items.length === 0">
            Выбрать аптеку
          </button>

          <div v-else class="selected-pharmacy-card">
            <div class="sp-header">
              <span class="sp-label">Пункт самовывоза:</span>
              <button class="sp-change" @click="isMapOpen = true">Изменить</button>
            </div>
            <p class="sp-address">{{ cartStore.selectedPharmacy.address }}</p>
            <p class="sp-name">{{ cartStore.selectedPharmacy.name }}</p>
            
            <button class="checkout-btn confirm-btn" @click="cartStore.checkout">
              Оформить заказ
            </button>
          </div>
        </div>
      </aside>

      <PharmacyModal 
        :is-open="isMapOpen" 
        @close="isMapOpen = false" 
        @select="handlePharmacySelect" 
      />

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useCartStore } from '@/stores/cartStore';
import TheHeader from '@/components/Header.vue';
import CartItem from '@/components/CartItem.vue';
import PharmacyModal from '@/components/PharmacyModal.vue';

const cartStore = useCartStore();
const isMapOpen = ref(false);

const handlePharmacySelect = (pharmacy) => {
  cartStore.setPharmacy(pharmacy);
  isMapOpen.value = false;
};

onMounted(async () => {
  await cartStore.loadCart();
});
</script>

<style scoped>
    .cart-page {
        padding: 40px 0;
        background: var(--background-color);
        min-height: 80vh;
        font-family: var(--main-font);
    }

    .container {
        max-width: 1200px;
        margin: 0 auto;
        padding: 0 20px;
    }

    .cart-layout {
        display: flex;
        gap: 30px;
        align-items: flex-start;
    }

    .cart-items-section {
        flex: 1;
        background: white;
        border-radius: 20px;
        padding: 30px;
        box-shadow: 0 4px 15px rgba(0,0,0,0.03);
    }

    .cart-header {
        display: flex;
        justify-content: space-between;
        align-items: baseline;
        margin-bottom: 25px;
    }

    .cart-header h2 {
        font-size: 28px;
        margin: 0;
        color: #000;
        display: inline-block;
        margin-right: 15px;
    }

    .items-count {
        color: #888;
        font-size: 14px;
    }

    .clear-btn {
        background: none;
        border: none;
        color: var(--accent-color);
        font-size: 16px;
        font-weight: 500;
        font-family: var(--main-font);
        cursor: pointer;
    }

    .clear-btn:hover {
        text-decoration: underline;
    }

    .empty-cart {
        text-align: center;
        padding: 50px 0;
        color: #888;
    }

    .go-catalog-btn {
        display: inline-block;
        margin-top: 15px;
        background: var(--primary-color);
        color: white;
        font-weight: medium;
        font-size: 20px;
        text-decoration: none;
        padding: 10px 20px;
        border-radius: 8px;
    }

    .cart-list {
        display: flex;
        flex-direction: column;
        gap: 20px;
    }

    .cart-summary {
        flex: 0 0 320px;
        background: white;
        border-radius: 20px;
        padding: 30px;
        box-shadow: 0 4px 15px rgba(0,0,0,0.03);
        position: sticky;
        top: 20px;
    }

    .cart-summary h3 {
        margin: 0 0 20px 0;
        font-size: 20px;
        color: #000;
    }

    .summary-row {
        display: flex;
        justify-content: space-between;
        font-size: 14px;
        color: #666;
        margin-bottom: 15px;
    }

    .summary-total {
        border-top: 1px solid #eee;
        padding-top: 15px;
        margin-top: 10px;
        font-weight: 600;
        color: #000;
    }

    .total-price-row {
        font-size: 16px;
        color: #000;
        margin-top: 10px;
        margin-bottom: 25px;
    }

    .total-price {
        font-size: 24px;
        font-weight: 700;
    }

    .checkout-btn {
        width: 100%;
        background: var(--primary-color);
        color: white;
        border: none;
        padding: 15px;
        border-radius: 12px;
        font-size: 20px;
        font-weight: 600;
        font-family: var(--main-font);
        cursor: pointer;
    }

    .checkout-btn:disabled {
        cursor: not-allowed;
        opacity: 0.5;
    }

    .pharmacy-selection {
        margin-top: 20px;
    }

    .select-map-btn {
        opacity: 1;
        transition: 0.2s;
    }

    .select-map-btn:hover:not(:disabled) {
        filter: brightness(0.9);
    }

    .selected-pharmacy-card {
        background: #F9F9F9;
        border-radius: 12px;
        padding: 15px;
        border: 1px solid #eee;
    }

    .sp-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 8px;
    }

    .sp-label {
        font-size: 12px;
        color: #888;
        font-weight: 600;
        text-transform: uppercase;
    }

    .sp-change {
        background: none;
        border: none;
        color: var(--primary-color);
        font-size: 12px;
        cursor: pointer;
        text-decoration: underline;
    }

    .sp-address {
        font-size: 14px;
        color: #333;
        font-weight: 600;
        margin: 0 0 5px 0;
    }

    .sp-name {
        font-size: 12px;
        color: #666;
        margin: 0 0 15px 0;
    }

    .confirm-btn {
        opacity: 1;
        margin-top: 10px;
    }

    @media (max-width: 800px) {
        .cart-items-section {
            width: 100%;
        }

        .cart-layout {
            flex-direction: column;
        }
        .cart-summary {
            width: 100%;
            position: static;
        }
    }
</style>