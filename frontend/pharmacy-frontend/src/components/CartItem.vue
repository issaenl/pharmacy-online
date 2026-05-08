<template>
  <div class="cart-item">
    <img :src="item.imageUrl || '/assets/no-image.jpg'" class="item-img" />
    
    <div class="item-info">
        <router-link :to="`/product/${item.productId}`" class="item-name">
            {{ item.productName }}
        </router-link>
        
        <div class="price-container">
          <template v-if="finalPrice > 0">
            <div v-if="item.discountPercentage" class="old-price">
              {{ item.unitPrice.toFixed(2) }} р.
            </div>
            
            <div class="item-price" :class="{ 'has-discount': item.discountPercentage }">
              <template v-if="!orderStore.selectedPharmacy">от </template>
              {{ finalPrice.toFixed(2) }} <span class="nbrb-icon nbrb-icon-byn"></span>
              
              <span v-if="item.discountPercentage" class="discount-badge">
                -{{ item.discountPercentage }}%
              </span>
            </div>
          </template>

          <template v-else>
            <div class="out-of-stock-label">Нет в наличии</div>
          </template>
        </div>
        </div>

        <div v-if="finalPrice > 0" class="quantity-control">
            <button @click="cartStore.updateQuantity(item.productId, item.quantity - 1)" :disabled="item.quantity <= 1">−</button>
            <span>{{ item.quantity }}</span>
            <button @click="cartStore.updateQuantity(item.productId, item.quantity + 1)">+</button>
            </div>

            <div v-else class="waitlist-action">
            <button class="waitlist-btn-small" @click="addToWaitlist">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="margin-right: 5px;">
                    <circle cx="12" cy="12" r="10"></circle>
                    <polyline points="12 6 12 12 16 14"></polyline>
                </svg>
                В лист ожидания
            </button>
         </div>

    <div class="item-actions">
      <button 
        class="action-btn" 
        :class="{ 'is-favorite': favoriteStore.isFavorite(item.productId) }"
        @click="favoriteStore.toggleFavorite(item)"
        title="В избранное">
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
        </svg>
      </button>

      <button @click="cartStore.removeFromCart(item.productId)" class="action-btn" title="Удалить">
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="3 6 5 6 21 6"></polyline>
          <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
          <line x1="10" y1="11" x2="10" y2="17"></line>
          <line x1="14" y1="11" x2="14" y2="17"></line>
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup>
import { defineProps, computed } from 'vue';
import { useCartStore } from '@/stores/cartStore';
import { useFavoriteStore } from '@/stores/favoriteStore';
import { useOrderStore } from '@/stores/orderStore';

const props = defineProps({
  item: {
    type: Object,
    required: true
  }
});

const cartStore = useCartStore();
const favoriteStore = useFavoriteStore();
const orderStore = useOrderStore();

const finalPrice = computed(() => {
  if (!props.item.discountPercentage) return props.item.unitPrice;
  const discountAmount = props.item.unitPrice * (props.item.discountPercentage / 100);
  return props.item.unitPrice - discountAmount;
});

const addToWaitlist = () => {
  alert(`Вы подписаны на уведомление о поступлении товара: ${props.item.productName}`);
};
</script>

<style scoped>
    .cart-item { 
        display: flex; 
        align-items: center; 
        padding-bottom: 20px; 
        border-bottom: 1px solid #F0F0F0; 
        gap: 20px; 
    }

    .cart-item:last-child { 
        border-bottom: none; 
        padding-bottom: 0; 
    }

    .item-img { 
        width: 80px; 
        height: 80px; 
        object-fit: contain; 
    }

    .item-info { 
        flex: 1; 
        display: flex; 
        flex-direction: column; 
        gap: 5px; 
    }

    .item-name { 
        color: #000; 
        font-weight: bold;
        font-size: 20px; 
        text-decoration: none;
    }

    .item-price { 
        color: #000; 
        font-size: 18px; 
        display: flex;
        align-items: center;
        gap: 8px;
    }

    .price-container {
        display: flex;
        flex-direction: column;
    }

    .old-price {
        font-size: 13px;
        color: #999;
        text-decoration: line-through;
        margin-bottom: 2px;
        line-height: 1;
    }

    .item-price.has-discount {
        color: #BB4E58;
        font-weight: 600;
    }

    .discount-badge {
        background: #BB4E58;
        color: white;
        padding: 2px 6px;
        border-radius: 6px;
        font-size: 12px;
        font-weight: bold;
        line-height: 1;
    }

    .quantity-control { 
        display: flex; 
        align-items: center; 
        background: var(--background-color); 
        border-radius: 20px; 
        padding: 5px 10px; 
        gap: 15px; 
    }

    .quantity-control button { 
        background: none; 
        border: none; 
        font-size: 18px; 
        color: #888; 
        cursor: pointer; 
        padding: 0 5px; 
    }

    .quantity-control button:disabled { 
        opacity: 0.3; 
        cursor: not-allowed; 
    }

    .quantity-control span { 
        font-weight: 600; 
        font-size: 16px; 
        min-width: 20px; 
        text-align: center; 
    }

    .item-actions { 
        display: flex; 
        flex-direction: column; 
        gap: 10px; 
        margin-left: 20px; 
    }
    
    .action-btn { 
        background: none; 
        border: none; 
        cursor: pointer; 
        padding: 5px; 
        opacity: 0.6; 
        transition: 0.2s; 
        color: #A0A0A0;
    }
    
    .action-btn:hover { 
        opacity: 1;
    }

    .action-btn.is-favorite {
        color: #BB4E58;
        opacity: 1;
    }

    .action-btn.is-favorite svg {
        fill: currentColor;
    }

    .cart-item.is-out-of-stock {
        background-color: #f9f9f9;
        opacity: 0.8;
    }

    .out-of-stock-label {
        color: #BB4E58;
        font-weight: bold;
        font-size: 16px;
    }

    .waitlist-btn-small {
        background-color: #888;
        color: white;
        border: none;
        border-radius: 12px;
        padding: 8px 12px;
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 6px;
        transition: 0.2s;
    }

    .waitlist-btn-small:hover {
        background-color: #3d4144;
    }

    .is-out-of-stock .item-name {
        color: #666;
    }

    @media (max-width: 800px) {
        .waitlist-action {
            width: 100%;
            margin-top: 10px;
        }
        
        .waitlist-btn-small {
            width: 100%;
            justify-content: center;
        }
    }

    @media (max-width: 800px) {

        .cart-item { 
            flex-wrap: wrap; 
        }

        .item-info {
            min-width: calc(100% - 100px);
        }

        .item-actions { 
            flex-direction: row; 
            margin-left: auto; 
            width: auto; 
            align-items: center;
        }
    }
</style>