<template>
  <div class="card" @click="goToProduct">
    <button class="wishlist-btn" @click.stop="toggleWishlist">
      <img src="/assets/HeartEmpty.svg" alt="Сердце">
    </button>
    
    <div class="card__image">
      <img :src="product.pictureUrl || '/assets/no-image.jpg'" alt="image"/>
    </div>

    <div class="card__info">
      <h4 class="title">{{ product.name }}</h4>
      <div class="dosage">{{ product.dosageForm }}</div>
      <div class="price">от {{ product.minPrice }} р.</div>
    </div>

    <button class="cart-btn" @click.stop="addToCart">
      <img src="/assets/Cart.svg" alt="Тележка" class="icon-cart">
      В корзину
    </button>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';

const props = defineProps({
  product: {
    type: Object,
    required: true,
    default: () => ({})
  }
});

const router = useRouter();

const goToProduct = () => {
  router.push(`/product/${props.product.id}`);
};

/*пока без логики позже реализую*/
const toggleWishlist = () => {};
const addToCart = () => {};
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
    font-size: 18px; 
    color: #000000; 
    margin: 0 0 5px; 
    height: 40px; 
    line-height: 1.2;
    height: 2.4em;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    overflow-wrap: break-word;
    word-wrap: break-word;
    word-break: break-word;
  }

  .dosage {
    font-size: 14px;
    color: #999;
    margin-bottom: 10px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    height: 1.2em;
  }

  /*не используется, для побочного текста типо количества в упаковке*/
  .meta { 
    font-size: 12px; 
    color: #999; 
    margin-bottom: 10px; 
  }

  .price { 
    font-size: 20px; 
    margin-bottom: 15px; 
  }

  .cart-btn {
    background: #BB4E58;
    color: white;
    border: none;
    width: 100%;
    padding: 10px;
    border-radius: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    cursor: pointer;
    font-family: var(--main-font);
    font-size: 20px;
    font-weight:600px;
  }

  .icon-cart { 
    width: 18px; 
    height: 18px; 
  }

  @media (max-width: 1024px) {
    .title { font-size: 16px; height: 42px; }
    .price { font-size: 18px; }
    .card__image { height: 150px; }
  }

  @media (max-width: 600px) {
    .card { padding: 12px; border-radius: 20px; }
    .card__image { height: 120px; margin-bottom: 10px; }
    .title { font-size: 14px; height: 36px; margin-bottom: 5px; }
    .price { font-size: 16px; margin-bottom: 10px; }
    .cart-btn { font-size: 14px; padding: 10px 5px; border-radius: 12px; }
    .cart-btn span { display: block; } 
    .wishlist-btn img { width: 20px; }
  }

  @media (max-width: 400px) {
    .cart-btn span { display: none; }
    
  }
</style>