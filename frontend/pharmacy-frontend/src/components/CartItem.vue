<template>
  <div class="cart-item">
    <img :src="item.pictureUrl || '/assets/no-image.jpg'" class="item-img" />
    
    <div class="item-info">
        <router-link :to="`/product/${item.productId}`" class="item-name">
            {{ item.productName }}
        </router-link>
        
        <div class="item-price">
          <template v-if="!orderStore.selectedPharmacy">от </template>
          {{ item.unitPrice.toFixed(2) }} р.
        </div>
    </div>


    <div class="quantity-control">
      <button @click="cartStore.updateQuantity(item.productId, item.quantity - 1)" :disabled="item.quantity <= 1">−</button>
      <span>{{ item.quantity }}</span>
      <button @click="cartStore.updateQuantity(item.productId, item.quantity + 1)">+</button>
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
import { defineProps } from 'vue';
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

    .item-price { 
        color: #000; 
        font-size: 18px; 
    }

    .item-name { 
        color: #000; 
        font-weight: bold;
        font-size: 20px; 
        text-decoration: none;
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