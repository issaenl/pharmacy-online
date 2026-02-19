<template>
  <div class="profile-orders-component">
    <div class="section-header">
      <h2>История заказов</h2>
    </div>
    
    <div class="empty-state">
      <img src="/assets/Cart.svg" alt="Пусто" class="empty-icon">
      <p>У вас пока нет оформленных заказов.</p>
      <router-link to="/full-catalog" class="go-catalog-btn">Перейти в каталог</router-link>
    </div>
  </div>
</template>

<style scoped>
    .section-header { 
        margin-bottom: 20px; 
    }

    .section-header h2 { 
        margin: 0; 
        font-size: 28px; 
        color: #000; 
    }

    .empty-state { 
        text-align: center; 
        background: white; 
        padding: 50px; 
        border-radius: 20px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.05); 
    }

    .empty-icon { 
        width: 64px; 
        height: 64px; 
        opacity: 0.3; 
        margin-bottom: 20px; 
    }

    .empty-state p { 
        font-size: 18px; 
        color: #666; 
        margin-bottom: 20px; 
    }

    .go-catalog-btn { 
        display: inline-block; 
        background: #689D6D; 
        color: white; 
        text-decoration: none; 
        padding: 12px 25px; 
        border-radius: 10px; 
        font-weight: 600; 
    }
</style>