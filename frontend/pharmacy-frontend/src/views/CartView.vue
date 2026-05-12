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
          <span class="total-price">
            <template v-if="!orderStore.selectedPharmacy">от </template>
            {{ cartStore.totalPrice.toFixed(2) }} р.
          </span>
        </div>

        <div class="prescription-warning" v-if="cartStore.items.length > 0">
          <svg class="warning-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="16" x2="12" y2="12"></line>
            <line x1="12" y1="8" x2="12.01" y2="8"></line>
          </svg>
          <p>Для выкупа рецептурных препаратов <strong>обязательно</strong> необходимо предъявить в аптеке действующий рецепт от врача.</p>
        </div>

        <div class="pharmacy-selection">
          <button 
            v-if="!orderStore.selectedPharmacy" 
            class="checkout-btn select-map-btn" 
            @click="isMapOpen = true"
            :disabled="cartStore.items.length === 0">
            Выбрать аптеку
          </button>

          <div v-else class="selected-pharmacy-card">
            <div class="sp-header">
              <span class="sp-label">Пункт самовывоза:</span>
              <div class="sp-header-actions">
                <button class="sp-change" @click="isMapOpen = true">Изменить</button>
                <button class="sp-reset" @click="handleResetPharmacy" title="Сбросить аптеку">✕</button>
              </div>
            </div>
            <p class="sp-address">{{ orderStore.selectedPharmacy.address }}</p>
            <p class="sp-name">{{ orderStore.selectedPharmacy.name }}</p>
            
            <button class="checkout-btn confirm-btn" @click="handleCheckout">
                Забронировать
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
import { useRouter } from 'vue-router';
import { useCartStore } from '@/stores/cartStore';
import { useAuthStore } from '@/stores/authStore';
import { useOrderStore } from '@/stores/orderStore';
import TheHeader from '@/components/Header.vue';
import CartItem from '@/components/CartItem.vue';
import PharmacyModal from '@/components/PharmacyModal.vue';
import { useToast } from 'vue-toast-notification';

const cartStore = useCartStore();
const authStore = useAuthStore();
const orderStore = useOrderStore();
const router = useRouter();
const isMapOpen = ref(false);
const toast = useToast({ position: 'bottom-right' });

const handlePharmacySelect = async (pharmacy) => {
  await orderStore.setPharmacy(pharmacy);
  isMapOpen.value = false;
};

const handleResetPharmacy = async () => {
  await orderStore.setPharmacy(null);
};

const handleCheckout = () => {
    if(!authStore.token){
        localStorage.setItem('redirectAfterLogin', '/cart');
        toast.info('Для оформления брони необходимо авторизироваться');
        router.push('/login');
    }
    else{
        orderStore.checkout();
    }
};

onMounted(async () => {
  await cartStore.loadCart();
});
</script>

<style scoped>
    .prescription-warning { display: flex; align-items: flex-start; gap: 12px; background-color: #FDE8E8; padding: 15px; border-radius: 12px; margin-bottom: 25px; }
    .warning-icon { width: 24px; height: 24px; color: #BB4E58; flex-shrink: 0; }
    .prescription-warning p { margin: 0; font-size: 18px; color: #BB4E58; line-height: 1.4; }
    .cart-page { padding: 40px 0; background: var(--background-color); min-height: 80vh; font-family: var(--main-font); }
    .container { max-width: 1200px; margin: 0 auto; padding: 0 20px; }
    .cart-layout { display: flex; gap: 30px; align-items: flex-start; }
    .cart-items-section { flex: 1; background: white; border-radius: 20px; padding: 30px; box-shadow: 0 4px 15px rgba(0,0,0,0.03); }
    .cart-header { display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 25px; }
    .cart-header h2 { font-size: 28px; margin: 0; color: #000; display: inline-block; margin-right: 15px; }
    .items-count { color: #888; font-size: 18px; }
    .clear-btn {  font-size: 18px; background: none; border: none; color: var(--accent-color); font-weight: 500; font-family: var(--main-font); cursor: pointer; }
    .clear-btn:hover { text-decoration: underline; }
    .empty-cart {  font-size: 22px; text-align: center; padding: 50px 0; color: #888; }
    .go-catalog-btn { display: inline-block; margin-top: 15px; background: var(--primary-color); color: white; font-weight: medium; font-size: 20px; text-decoration: none; padding: 10px 20px; border-radius: 8px; }
    .cart-list { display: flex; flex-direction: column; gap: 20px; }
    .cart-summary { flex: 0 0 320px; background: white; border-radius: 20px; padding: 30px; box-shadow: 0 4px 15px rgba(0,0,0,0.03); position: sticky; top: 20px; }
    .cart-summary h3 { margin: 0 0 20px 0; font-size: 22px; color: #000; }
    .summary-row { display: flex; justify-content: space-between; font-size: 22px; color: #666; margin-bottom: 15px; }
    .summary-total { border-top: 1px solid #eee; padding-top: 15px; margin-top: 10px; font-weight: 600; color: #000; }
    .total-price-row { font-size: 22px; color: #000; margin-top: 10px; margin-bottom: 25px; }
    .total-price { font-size: 28px; font-weight: 700; }
    .checkout-btn { width: 100%; background: var(--primary-color); color: white; border: none; padding: 15px; border-radius: 12px; font-size: 20px; font-weight: 600; font-family: var(--main-font); cursor: pointer; }
    .checkout-btn:disabled { cursor: not-allowed; opacity: 0.5; }
    .pharmacy-selection { margin-top: 20px; }
    .select-map-btn { opacity: 1; transition: 0.2s; }
    .select-map-btn:hover:not(:disabled) { filter: brightness(0.9); }
    .selected-pharmacy-card { background: #F9F9F9; border-radius: 12px; padding: 15px; border: 1px solid #eee; }
    
    .sp-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
    .sp-label { font-size: 14px; color: #888; font-weight: 600; text-transform: uppercase; }
    
    .sp-header-actions { display: flex; align-items: center; gap: 10px; }
    .sp-change { background: none; border: none; color: var(--primary-color); font-size: 18px; font-family: var(--main-font); cursor: pointer; text-decoration: underline; padding: 0; }
    .sp-reset { background: none; border: none; color: #999; font-size: 16px; font-weight: bold; cursor: pointer; padding: 0; line-height: 1; transition: 0.2s; }
    .sp-reset:hover { color: var(--accent-color, #BB4E58); }

    .sp-address { font-size: 18px; color: #333; font-weight: 600; margin: 0 0 5px 0; }
    .sp-name { font-size: 18px; color: #555; margin: 0 0 15px 0; }
    .confirm-btn { opacity: 1; margin-top: 10px; }

    @media (max-width: 800px) {
        .cart-items-section { width: 100%; }
        .cart-layout { flex-direction: column; }
        .cart-summary { width: 100%; position: static; }
    }
</style>