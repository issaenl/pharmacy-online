<template>
  <div class="admin-dashboard">
    <div class="header-actions">
      <h2>Обзорная панель</h2>
      <button class="btn-primary refresh-btn" @click="fetchStats" :disabled="isLoading">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" :class="{ 'spin': isLoading }"><polyline points="23 4 23 10 17 10"></polyline><polyline points="1 20 1 14 7 14"></polyline><path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path></svg>
        Обновить
      </button>
    </div>

    <div v-if="isLoading && !stats" class="loading-state">
      Загрузка данных...
    </div>

    <div v-else-if="stats" class="dashboard-content">
      
      <div class="metrics-grid">
        <div class="metric-card">
          <div class="metric-icon money">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="6" width="20" height="12" rx="2"></rect><circle cx="12" cy="12" r="2"></circle><path d="M6 12h.01M18 12h.01"></path></svg>
          </div>
          <div class="metric-info">
            <div class="metric-label">Выручка за текущий день</div>
            <div class="metric-value">{{ formatPrice(revenue) }} р.</div>
          </div>
        </div>

        <div class="metric-card">
          <div class="metric-icon orders">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"></path><line x1="3" y1="6" x2="21" y2="6"></line><path d="M16 10a4 4 0 0 1-8 0"></path></svg>
          </div>
          <div class="metric-info">
            <div class="metric-label">{{ isSuperAdmin ? 'Всего заказов' : 'Выдано заказов' }}</div>
            <div class="metric-value">{{ ordersCount }}</div>
          </div>
        </div>

        <div class="metric-card">
          <div class="metric-icon" :class="isSuperAdmin ? 'pharmacy' : 'pending'">
            <svg v-if="isSuperAdmin" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path><polyline points="9 22 9 12 15 12 15 22"></polyline></svg>
            <svg v-else width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg>
          </div>
          <div class="metric-info">
            <div class="metric-label">{{ isSuperAdmin ? 'Активных аптек' : 'Ожидают сборки' }}</div>
            <div class="metric-value">{{ isSuperAdmin ? stats.activePharmacies : stats.pendingOrders }}</div>
          </div>
        </div>

       <div class="metric-card alert">
        <div class="metric-icon warning">
            <svg 
            width="24" 
            height="24" 
            viewBox="0 0 24 24" 
            fill="none" 
            stroke="currentColor" 
            stroke-width="2" 
            stroke-linecap="round" 
            stroke-linejoin="round" 
            style="overflow: visible;"
            >
            <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
            <line x1="12" y1="9" x2="12" y2="13"></line>
            <line x1="12" y1="17" x2="12.01" y2="17"></line>
            </svg>
        </div>
        <div class="metric-info">
            <div class="metric-label">Заканчивается (менее 5 шт)</div>
            <div class="metric-value danger">{{ lowStock }} поз.</div>
        </div>
        </div>
      </div>

      <div class="tables-grid">

        <div class="dashboard-panel panel-danger">
          <h3 class="panel-title">Дефицит товаров (менее 5 шт)</h3>
          <div class="list-container">
            <div v-for="(prod, index) in stats.lowStockProducts" :key="'low-'+index" class="list-item">
              <div class="item-left">
                <div class="item-name-block">
                  <span class="item-name">{{ prod.productName }}</span>
                  <span v-if="isSuperAdmin" class="meta-pharmacy">{{ prod.pharmacyName }}</span>
                </div>
              </div>
              <div class="item-right">
                <span class="qty-badge danger-text">{{ prod.quantity }} шт.</span>
              </div>
            </div>
            <div v-if="!stats.lowStockProducts?.length" class="empty-state success-text">
              Везде полный склад
            </div>
          </div>
        </div>

        <div class="dashboard-panel panel-danger">
          <h3 class="panel-title">Срок годности истекает в ближайший месяц</h3>
          <div class="list-container">
            <div v-for="(prod, index) in stats.expiringProducts" :key="'exp-'+index" class="list-item">
              <div class="item-left">
                <div class="item-name-block">
                  <span class="item-name">{{ prod.productName }}</span>
                  <span v-if="isSuperAdmin && prod.pharmacyName" class="meta-pharmacy">{{ prod.pharmacyName }}</span>
                </div>
              </div>
              <div class="item-right">
                <span class="date-badge">{{ formatDate(prod.expirationDate) }}</span>
              </div>
            </div>
            <div v-if="!stats.expiringProducts?.length" class="empty-state success-text">
              Все товары в норме
            </div>
          </div>
        </div>
        
        <div class="dashboard-panel">
          <h3 class="panel-title">Топ-5 продаваемых товаров в этом месяце</h3>
          <div class="list-container">
            <div v-for="(prod, index) in stats.topProducts" :key="'top-'+index" class="list-item">
              <div class="item-left">
                <span class="rank-badge">{{ index + 1 }}</span>
                <span class="item-name">{{ prod.productName }}</span>
              </div>
              <div class="item-right">
                <span class="qty-badge">{{ prod.totalSold }} шт.</span>
              </div>
            </div>
            <div v-if="!stats.topProducts?.length" class="empty-state">Нет продаж за период</div>
          </div>
        </div>

      </div> 
    </div> 
 </div> 
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { useToast } from 'vue-toast-notification';
import api from '@/api/api';

const authStore = useAuthStore();
const toast = useToast({ position: 'bottom-right' });

const stats = ref(null);
const isLoading = ref(false);

const isSuperAdmin = computed(() => authStore.user?.role === 2 || authStore.user?.role === 'Admin');

const revenue = computed(() => stats.value?.totalRevenue ?? stats.value?.revenueToday ?? 0);
const ordersCount = computed(() => stats.value?.ordersToday ?? stats.value?.completedTodayOrders ?? 0);
const lowStock = computed(() => stats.value?.globalLowStockCount ?? stats.value?.lowStockCount ?? 0);

const fetchStats = async () => {
  isLoading.value = true;
  try {
    if (isSuperAdmin.value) {
      const response = await api.get('/Admin/admin');
      stats.value = response.data;
    } else {
      const pharmacyId = authStore.user?.pharmacyId || 1;
      const response = await api.get(`/Admin/pharmacy/${pharmacyId}`); 
      stats.value = response.data;
    }
  } catch (error) {
    toast.error("Не удалось загрузить статистику");
    console.error(error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchStats();
});

const formatPrice = (price) => {
  if (price == null) return '0.00';
  return Number(price).toFixed(2);
};

const formatDate = (dateString) => {
  if (!dateString) return 'н/д';
  return new Date(dateString).toLocaleDateString('ru-RU', {
    day: '2-digit', month: '2-digit', year: 'numeric'
  });
};
</script>

<style scoped>
.admin-dashboard {
  padding: 20px;
  font-family: var(--main-font);
}

.header-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.header-actions h2 {
  margin: 0;
  color: #000;
  font-size: 28px;
}

.refresh-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 16px;
  padding: 10px 20px;
  border-radius: 12px;
  background: var(--primary-color);
  color: #fff;
  border: none;
  cursor: pointer;
  transition: 0.2s;
}

.refresh-btn:hover {
  background: #558559;
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  100% { transform: rotate(360deg); }
}

.loading-state {
  text-align: center;
  padding: 50px;
  color: #888;
  font-size: 18px;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.metric-card {
  background: #fff;
  border-radius: 20px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 20px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.03);
  transition: 0.3s;
}

.metric-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.06);
}

.metric-card.alert {
  border: 1px solid #FDE8E8;
  background: #fffcfc;
}

.metric-icon {
  width: 60px;
  height: 60px;
  border-radius: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.metric-icon.money { background: #E8F4EA; color: #689D6D; }
.metric-icon.orders { background: #Eef2ff; color: #3b82f6; }
.metric-icon.pharmacy { background: #f3faf1; color: #B3CCAE; }
.metric-icon.pending { background: #fef3c7; color: #f0d159; }
.metric-icon.warning { background: #FDE8E8; color: #BB4E58; }

.metric-info {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.metric-label {
  font-size: 16px;
  color: #888;
  font-weight: 500;
}

.metric-value {
  font-size: 26px;
  font-weight: 700;
  color: #000;
}

.metric-value.danger {
  color: #BB4E58;
}

.tables-grid {
  display: flex;
  flex-direction: column;
  gap: 30px;
}

.dashboard-panel {
  background: #fff;
  border-radius: 20px;
  padding: 25px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.03);
}

.panel-danger {
  border-top: 4px solid #BB4E58;
}

.panel-title {
  margin: 0 0 20px 0;
  font-size: 18px;
  font-weight: 600;
  color: #333;
}

.list-container {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.list-item {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  padding: 12px 20px;
  background: #fafafa;
  border-radius: 12px;
  transition: 0.2s;
}

.list-item:hover {
  background: #f1f1f1;
}

.item-left {
  display: flex;
  align-items: center;
  gap: 15px;
  flex: 0 0 400px;
  max-width: 100%;
}

.item-right {
  text-align: left;
}

.rank-badge {
  width: 28px;
  height: 28px;
  background: var(--primary-light);
  color: var(--primary-color);
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  font-weight: bold;
  font-size: 14px;
}

.item-name {
  font-weight: 600;
  font-size: 16px;
  color: #000000;
}

.qty-badge {
  font-weight: 700;
  font-size: 15px;
  color: var(--primary-color);
}

.danger-text {
  color: #BB4E58 !important;
}

.date-badge {
  background: #FDE8E8;
  color: #BB4E58;
  padding: 4px 10px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
}

.item-name-block {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.meta-pharmacy {
  font-size: 14px;
  color: #888;
}

.empty-state {
  text-align: center;
  padding: 30px;
  color: #999;
  font-size: 15px;
  background: #fafafa;
  border-radius: 12px;
  border: 1px dashed #ddd;
}

.success-text {
  color: var(--primary-color) !important;
  border-color: var(--primary-light) !important;
  background: #E8F4EA !important;
}

@media (max-width: 600px) {
  .list-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }
  .item-left {
    flex: 1 1 auto;
  }
}
</style>