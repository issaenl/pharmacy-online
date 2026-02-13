<template>
  <header class="header">
    <div class="header__main">
      <div class="container main-content">
        <div class="logo">УниМед</div>
        
        <button class="catalog-btn">
            <img src="/assets/FilterLeft.svg" alt="Каталог">
            <span class="catalog-text">Каталог</span>
        </button>

        <div class="search-bar">
        <input type="text" placeholder="Поиск лекарств" />
        <button class="search-btn">Найти</button>
    </div>

        <div class="user-menu">
            <div class="menu-item">
                <img src="/assets/Heart.svg" alt="Сердце" class="icon-img">
                <span>Избранное</span>
            </div>
            <div class="menu-item">
                <img src="/assets/Basket.svg" alt="Корзина" class="icon-img">
                <span>Корзина</span>
            </div>
            <div class="menu-item">
                <img src="/assets/User.svg" alt="Пользватель" class="icon-img">
                <span>Войти</span>
            </div>
        </div>
      </div>
    </div>

    <nav class="header__nav">
      <div class="container nav-list">
        <a href="#">Лекарства</a>
        <a href="#">Витамины и БАДы</a>
        <a href="#">Медицинские изделия</a>
        <a href="#">Гигиена</a>
        <a href="#">Косметика</a>
      </div>
    </nav>
  </header>
</template>

<style scoped>
    .header__main { 
        width: 100%;
        padding: 25px;
        background: #689D6D; 
        border-bottom: 1px solid #fff;
    }

    .container {
        max-width: 1200px; 
        margin: 0 auto;
        padding: 0 20px;
        display: flex;
        align-items: center;
    }

    .main-content { 
        justify-content: space-between;
        gap: 20px; 
    }
    
    .logo { 
        font-size: 28px; 
        font-weight: bold; 
        color: #fff; 
        font-family: var(--logo-font);
        font-size: 36px;
        cursor: pointer;
    }

    .catalog-btn {
        background: #B3CCAE;
        color: white;
        border: none;
        padding: 10px 20px;
        border-radius: 10px;
        display: flex;
        align-items: center;
        gap: 8px;
        cursor: pointer;
        font-family: var(--main-font);
        font-size: 20px;
    }

    .catalog-text{
        font-size:20px;
        font-weight: 500;
    }

    .search-bar { 
        display: flex; 
        flex: 1;
    }

    .search-bar input {
        flex: 1;
        padding: 10px;
        border: none;
        border-radius: 10px 0 0 10px;
        font-family: var(--main-font);
        font-size: 20px;
        font-weight: 500;
    }

    .search-btn {
        background: #B3CCAE;
        color: white;
        border: none;
        padding: 0 20px;
        border-radius: 0 10px 10px 0;
        font-family: var(--main-font);
        font-size: 20px;
        font-weight: 500;
        cursor: pointer;
    }

    .user-menu { 
        display: flex; 
        gap: 20px; 
    }

    .menu-item { 
        display: flex;
        flex-direction: column;
        align-items:center;
        text-align: center; 
        font-size: 16px; 
        font-weight: 500;
        color: #fff; 
        cursor: pointer; 
    }

    .icon-img {
        width: 24px;
        height: 24px;
        object-fit: contain;
        display: block;
        margin: 0 auto 4px;
    }

    .header__nav { 
        background: #689D6D; 
        color: white; 
        padding: 20px 0; 
    }

    .nav-list { 
        display: flex; 
        gap: 30px; 
        justify-content: center; 
        font-size: 14px; 
    }

    .nav-list a { 
        font-family: var(--main-font);
        font-weight: 600;
        font-size: 16px;
        color: white; 
        text-decoration: none; 
    }
</style>