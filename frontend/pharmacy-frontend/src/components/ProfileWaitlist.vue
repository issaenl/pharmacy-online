<template>
  <div class="profile-tab-content">
    <div class="header-actions">
      <h2 class="tab-title">Мой лист ожидания</h2>
      
      <button 
        v-if="waitlistStore.items.length > 0" 
        class="btn-outline clear-btn" 
        @click="confirmClearAll"
        :disabled="waitlistStore.isLoading">
        Очистить всё
      </button>
    </div>

    <div v-if="waitlistStore.isLoading" class="loading-state">
      <div class="spinner"></div>
      <p>Загрузка списка...</p>
    </div>

    <div v-else-if="waitlistStore.items.length === 0" class="empty-state">
      <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="#ccc" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
        <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
        <polyline points="22 4 12 14.01 9 11.01"></polyline>
      </svg>
      <h3>Ваш лист ожидания пуст</h3>
      <p>Добавляйте товары из каталога, которых сейчас нет в наличии. Мы пришлем уведомление, когда они появятся в аптеках вашего региона.</p>
      <router-link to="/full-catalog" class="btn-primary go-catalog-btn">
        Перейти в каталог
      </router-link>
    </div>

    <div class="waitlist-container" v-else>
      <div class="cart-item" v-for="item in waitlistStore.items" :key="item.id">
        <img :src="item.pictureUrl || '/assets/no-image.jpg'" class="item-img" alt="Product image"/>
        
        <div class="item-info">
          <router-link :to="`/product/${item.productId}`" class="item-name">
            {{ item.productName }}
          </router-link>
          
          <div class="waitlist-meta">
            <span class="district-badge">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right: 4px;">
                <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                <circle cx="12" cy="10" r="3"></circle>
              </svg>
              {{ item.district }}
            </span>
            <span class="date-added">Добавлено: {{ formatDate(item.addedAt) }}</span>
          </div>
        </div>

        <div class="item-actions">
          <button @click="removeItem(item.id)" class="action-btn" title="Удалить из листа ожидания">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6"></polyline>
              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
              <line x1="10" y1="11" x2="10" y2="17"></line>
              <line x1="14" y1="11" x2="14" y2="17"></line>
            </svg>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue';
import { useWaitlistStore } from '@/stores/waitlistStore';

const waitlistStore = useWaitlistStore();

onMounted(() => {
  waitlistStore.fetchWaitlist();
});

const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('ru-RU', { 
    day: '2-digit', 
    month: '2-digit', 
    year: 'numeric' 
  });
};

const removeItem = async (id) => {
  if (confirm('Удалить этот товар из листа ожидания? Вы больше не получите уведомление о его поступлении.')) {
    await waitlistStore.removeFromWaitlist(id);
  }
};

const confirmClearAll = async () => {
  if (confirm('Вы уверены, что хотите полностью очистить лист ожидания?')) {
    await waitlistStore.clearWaitlist();
  }
};
</script>

<style scoped>
.profile-tab-content {
  background: white;
  border-radius: 20px;
  padding: 30px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
  min-height: 400px;
}

.header-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
  padding-bottom: 15px;
  border-bottom: 1px solid #eee;
}

.tab-title {
  margin: 0;
  font-size: 24px;
  color: #000;
  font-weight: 700;
}

.clear-btn {
  border: #BB4E58 2px solid;
  color: #BB4E58;
  padding: 8px 16px;
  font-family: var(--main-font);
  font-weight: 600;
  font-size: 16px;
  border-radius: 12px;
  background: transparent;
  cursor: pointer;
}

.clear-btn:hover {
  background: #fff5f6;
}

.waitlist-container {
  display: flex;
  flex-direction: column;
}

.cart-item { 
  display: flex; 
  align-items: center; 
  padding-bottom: 20px;
  padding-top: 20px; 
  border-bottom: 1px solid #F0F0F0; 
  gap: 20px; 
}

.cart-item:first-child {
  padding-top: 0;
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
  gap: 8px; 
}

.item-name { 
  color: #000; 
  font-weight: bold;
  font-size: 20px; 
  text-decoration: none;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.waitlist-meta {
  display: flex;
  align-items: center;
  gap: 15px;
  flex-wrap: wrap;
}

.district-badge {
  display: inline-flex;
  align-items: center;
  background: #e8f4ea;
  color: #689D6D;
  padding: 4px 10px;
  border-radius: 8px;
  font-size: 18px;
  font-weight: 600;
}

.date-added {
  color: #999;
  font-size: 16px;
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
  color: #555;
}

.action-btn:hover { 
  opacity: 1;
  transform: scale(1.1);
}

.loading-state, .empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 50px 20px;
  text-align: center;
}

.empty-state svg {
  margin-bottom: 20px;
}

.empty-state h3 {
  font-size: 20px;
  color: #333;
  margin-bottom: 10px;
}

.empty-state p {
  color: #888;
  max-width: 400px;
  margin-bottom: 25px;
  line-height: 1.5;
}

.go-catalog-btn {
  padding: 12px 30px;
  font-size: 16px;
  border-radius: 15px;
  text-decoration: none;
  display: inline-block;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #689D6D;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 15px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
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