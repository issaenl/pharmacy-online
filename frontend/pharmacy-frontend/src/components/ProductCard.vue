<template>
  <div class="card">
    <button class="wishlist-btn">
      <img src="/assets/HeartEmpty.svg" alt="Сердце">
    </button>
    
    <div class="card__image">
      <img :src="product.pictureUrl || '/assets/no-image.jpg'" alt="image"/>
    </div>

    <div class="card__info">
      <h4 class="title">{{ product.name }}, {{ product.dosageForm }}</h4>
      <div class="price">от {{ product.minPrice }} р.</div>
    </div>

    <button class="cart-btn">
      <img src="/assets/Cart.svg" alt="Тележка" class="icon-cart">
      В корзину
    </button>
  </div>
</template>

<script setup>
const props = defineProps({
  product: {
    type: Object,
    required: true,
    default: () => ({})
  }
});
</script>

<style scoped>
  .card {
    background: #fff;
    border-radius: 35px;
    padding: 25px 20px;
    text-align: left;
    position: relative;
    transition: 0.3s;
    border: 1px solid #fff;
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
    font-size: 20px; 
    color: #000000; 
    margin: 0 0 5px; 
    height: 50px; 
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
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

  .icon-small, .icon-cart { 
    width: 18px; 
    height: 18px; 
  }
</style>