<template>
  <footer class="site-footer">
    <div class="footer-container">
      <div class="footer-top">
        <div class="footer-column">
          <h3 class="footer-title">Контактная информация</h3>
          <ul class="footer-list contact-list">
            <li>
              <svg width="16" height="16" viewBox="0 0 24 24" fill="white" xmlns="http://www.w3.org/2000/svg">
                <path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/>
              </svg>
              <span>+375 (33) 614-64-18</span>
            </li>
            <li>
              <svg width="16" height="16" viewBox="0 0 24 24" fill="white" xmlns="http://www.w3.org/2000/svg">
                <path d="M20 4H4C2.9 4 2.01 4.9 2.01 6L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/>
              </svg>
              <span>unimedpharmacy@gmail.com</span>
            </li>
          </ul>
        </div>

        <div class="footer-column">
          <h3 class="footer-title">Навигация</h3>
          <ul class="footer-list">
            <li><router-link to="/full-catalog">Каталог</router-link></li>
            <li><router-link to="/profile">Профиль</router-link></li>
            <li><router-link to="/cart">Корзина</router-link></li>
            <li><router-link to="/pharmacies">Аптеки</router-link></li>
            <!-- <li><router-link to="/info">Информация</router-link></li> -->
          </ul>
        </div>

        <!-- <div class="footer-column">
          <h3 class="footer-title">Другое</h3>
          <ul class="footer-list">
            <li><router-link to="/for-pharmacies">Информация для аптек</router-link></li>
          </ul>
        </div> -->
      </div>

      <div class="footer-bottom">
        <p class="copyright">© 2026 UniMed. Все права защищены.</p>
        <div class="footer-links">
        </div>
      </div>
    </div>
  </footer>
</template>

<style scoped>
.site-footer {
  background-color: var(--accent-color);
  color: #ffffff;
  padding: 60px 0 30px;
  font-family: var(--main-font); 
}

.footer-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.footer-top {
  display: flex;
  justify-content: flex-start;
  gap: 120px;
  margin-bottom: 50px;
}

.footer-column {
  display: flex;
  flex-direction: column;
}

.footer-title {
  font-size: 18px;
  font-weight: bold;
  margin-bottom: 20px;
  color: #ffffff;
}

.footer-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.footer-list a {
  color: #ffffff;
  text-decoration: none;
  font-size: 14px;
  transition: opacity 0.2s ease;
}

.footer-list a:hover {
  opacity: 0.8;
}

.contact-list li {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 14px;
}

.footer-bottom {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 25px;
  border-top: 1px solid rgba(255, 255, 255, 0.2);
  font-size: 12px;
}

.copyright {
  margin: 0;
  color: #ffffff;
}

.footer-links {
  display: flex;
  gap: 30px;
}

.footer-links a {
  color: #ffffff;
  text-decoration: none;
  transition: opacity 0.2s ease;
}

.footer-links a:hover {
  opacity: 0.8;
}

@media (max-width: 768px) {
  .footer-top {
    flex-direction: column;
    gap: 40px;
  }
  
  .footer-bottom {
    flex-direction: column;
    gap: 15px;
    align-items: flex-start;
  }
}
</style>