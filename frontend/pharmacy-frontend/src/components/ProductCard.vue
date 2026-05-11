<template>
  <div class="card" @click="goToProduct">
    <button 
      class="wishlist-btn" 
      :class="{ 'is-favorite': favoriteStore.isFavorite(product.id) }"
      @click.stop="toggleWishlist">
      <svg 
      width="32" 
      height="32" 
      viewBox="0 0 24 24" 
      fill="none" 
      stroke="currentColor" 
      stroke-width="2" 
      stroke-linecap="round" 
      stroke-linejoin="round">
        <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
      </svg>
    </button>
    
    <div class="card__image">
       <img 
          :src="product.pictureUrl || '/assets/no-image.jpg'" 
          :alt="product.name"
          @error="$event.target.src = '/assets/no-image.jpg'">
    </div>

    <div class="card__info">
      <div class="card__text-block">
        <h4 class="title">{{ product.name }}</h4>
        <div class="dosage">{{ product.dosageForm }}</div>
      </div>

      <div v-if="product.expirationDate" class="expiration-date">
        Годен до: {{ formatExpDate(product.expirationDate) }}
      </div>

      <div class="price">
        <template v-if="product.minPrice > 0">
          <span v-if="!exactPrice">от </span>{{ formatPrice(product.minPrice) }} 
          <span class="nbrb-icon nbrb-icon-byn"></span>
        </template>
        <template v-else>
          <span class="out-of-stock">Нет в наличии</span>
        </template>
      </div>
    </div>

    <button 
      class="cart-btn" 
      :class="{ 'waitlist-mode': !product.minPrice || product.minPrice <= 0 }"
      @click.stop="product.minPrice > 0 ? addToCart() : openWaitlistModal()">
      <svg 
        v-if="product.minPrice > 0" 
        width="24" 
        height="24" 
        viewBox="0 0 24 24" 
        fill="none" 
        class="cart-svg">
        <path d="M0 2.25C0 2.05109 0.0790176 1.86032 0.21967 1.71967C0.360322 1.57902 0.551088 1.5 0.75 1.5H3C3.1673 1.50005 3.32978 1.55603 3.4616 1.65904C3.59342 1.76205 3.68701 1.90618 3.7275 2.0685L4.335 4.5H21.75C21.8609 4.50007 21.9704 4.52474 22.0707 4.57223C22.1709 4.61972 22.2594 4.68885 22.3297 4.77465C22.4 4.86044 22.4504 4.96076 22.4772 5.06838C22.5041 5.176 22.5067 5.28823 22.485 5.397L20.985 12.897C20.9522 13.0605 20.8658 13.2085 20.7395 13.3174C20.6131 13.4264 20.4541 13.4901 20.2875 13.4985L6.192 14.2065L6.6225 16.5H19.5C19.6989 16.5 19.8897 16.579 20.0303 16.7197C20.171 16.8603 20.25 17.0511 20.25 17.25C20.25 17.4489 20.171 17.6397 20.0303 17.7803C19.8897 17.921 19.6989 18 19.5 18H6C5.82515 17.9998 5.65585 17.9386 5.52137 17.8268C5.38688 17.7151 5.29567 17.5599 5.2635 17.388L3.015 5.4105L2.415 3H0.75C0.551088 3 0.360322 2.92098 0.21967 2.78033C0.0790176 2.63968 0 2.44891 0 2.25ZM4.653 6L5.913 12.7185L19.629 12.03L20.835 6H4.653Z" fill="currentColor"/>
        <circle cx="7.5" cy="21" r="1.5" fill="currentColor"/>
        <circle cx="18" cy="21" r="1.5" fill="currentColor"/>
      </svg>
      
      
      <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="margin-right: 5px;">
        <circle cx="12" cy="12" r="10"></circle>
        <polyline points="12 6 12 12 16 14"></polyline>
      </svg>

      <span>{{ product.minPrice > 0 ? 'В корзину' : 'В лист ожидания' }}</span>
    </button>
    
    <WaitlistModal 
      v-if="isModalOpen"
      :is-open="isModalOpen" 
      :product-id="product.id" 
      @close="isModalOpen = false" 
    />
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import WaitlistModal from '@/components/WaitlistModal.vue';
import { useRouter } from 'vue-router';
import { useCartStore } from '@/stores/cartStore';
import { useFavoriteStore } from '@/stores/favoriteStore';

const props = defineProps({
  product: {
    type: Object,
    required: true,
    default: () => ({})
  },
  exactPrice: {
    type: Boolean,
    default: false
  }
});

const router = useRouter();
const cartStore = useCartStore();
const favoriteStore = useFavoriteStore();
const authStore = useAuthStore();

const isModalOpen = ref(false);

const goToProduct = () => {
  router.push(`/product/${props.product.id}`);
};

const toggleWishlist = () => {
  favoriteStore.toggleFavorite(props.product);
};

const addToCart = () => {
  cartStore.addToCart({
    id: props.product.id,
    name: props.product.name,
    price: props.product.minPrice, 
    imageUrl: props.product.pictureUrl
  });
};

const formatPrice = (price) => {
  if (price == null) return '0.00';
  return Number(price).toFixed(2);
};

const formatExpDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const openWaitlistModal = () => {
  if (!authStore.token) {
    router.push('/login');
    return;
  }
  isModalOpen.value = true;
};
</script>

<style scoped>
  .card {
    background: #fff;
    border-radius: 25px;
    padding: 25px 20px;
    text-align: left;
    position: relative;
    transition: 0.3s;
    display: flex;
    flex-direction: column;
    height: 100%;
  }

  .card:hover { 
    box-shadow: 0 10px 20px rgba(0,0,0,0.05); 
  }

  .wishlist-btn {
    position: absolute;
    top: 15px;
    left: 15px;
    background: none;
    border: none;
    cursor: pointer;
    padding: 0;
    z-index: 2;
    color: #B4AFAC; 
    transition: color 0.2s ease;
  }

  .wishlist-btn:hover {
    color: #BB4E58; 
  }

  .wishlist-btn.is-favorite {
    color: #BB4E58; 
  }

  .wishlist-btn.is-favorite svg {
    fill: currentColor; 
  }

  .card__image { 
    height: 228px; 
    margin-bottom: 15px; 
    display: flex; 
    align-items: center; 
    justify-content: center; 
  }

  .card__image img { 
    max-height: 100%; 
    max-width: 100%; 
    object-fit: contain; 
  }

  .card__info {
    flex-grow: 1;
    display: flex;
    flex-direction: column;
    justify-content: flex-start;
  }

  .title { 
    font-size: 20px; 
    color: #000000; 
    margin: 0 0 5px; 
    line-height: 1.2;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    overflow-wrap: break-word;
    word-wrap: break-word;
    word-break: break-word;
  }

  .dosage {
    font-size: 16px;
    color: #999;
    line-height: 1.2;
    margin: 0;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    overflow-wrap: break-word;
    word-wrap: break-word;
    word-break: break-word;
  }

  .card__text-block {
    min-height: 70px; 
  }

  /*не используется, для побочного текста типо количества в упаковке*/
  .meta { 
    font-size: 12px; 
    color: #999; 
    margin-bottom: 10px; 
  }

  .expiration-date { 
    display: inline-flex; 
    align-items: center; 
    font-size: 16px; 
    font-weight: 600; 
    background: #E8F4EA;
    color: #689D6D;
    padding: 4px 8px; 
    border-radius: 6px; 
    margin-bottom: 10px;
    width: fit-content; 
  }

  .price { 
    font-size: 22px; 
    margin-bottom: 15px;
    margin-top: auto; 
  }

  .cart-btn {
    background: #BB4E58;
    color: white;
    border: none;
    width: 100%;
    padding: 10px;
    border-radius: 15px;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    cursor: pointer;
    font-family: var(--main-font);
    font-size: 20px;
    font-weight:600px;
  }

  .out-of-stock {
    color: #BB4E58;
    font-size: 18px;
    font-weight: 700;
    text-transform: uppercase;
  }

  .cart-btn:disabled {
    background-color: #888;
    cursor: pointer; 
  }


  .cart-btn.waitlist-mode {
    background-color: #888;
  }

  .cart-btn.waitlist-mode:hover {
    filter: brightness(0.9);
  }

  .cart-btn.waitlist-mode .cart-svg {
    display: none;
  }

  @media (max-width: 1024px) {
    .title 
    { 
      font-size: 20px; 
    }

    .price 
    { 
      font-size: 20px; 
    }

    .card__image 
    { 
      height: 150px; 
    }

    .card__text-block 
    {
      min-height: 60px;
    }
  }

  @media (max-width: 600px) {
    .card 
    { 
      padding: 12px; 
      border-radius: 20px; 
    }

    .card__image 
    { 
      height: 120px; 
      margin-bottom: 10px; 
    }

    .title 
    { 
      font-size: 18px; 
      margin-bottom: 5px; 
    }

    .price 
    { 
      font-size: 20px; 
      margin-bottom: 10px; 
    }

    .cart-btn 
    { 
      font-size: 16px; 
      padding: 10px 5px; 
    }

    .cart-btn span 
    { 
      display: block; 
    } 

    .wishlist-btn img 
    { 
      width: 20px; 
    }

    .card__text-block 
    {
      min-height: 55px;
    }
  }

</style>