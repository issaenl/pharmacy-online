<template>
  <div class="pharmacy-row">
    <div class="pharmacy-row__info">
      <img src="https://via.placeholder.com/50" class="pharmacy-row__img" alt="logo" />
      <div class="pharmacy-row__address-block">
        <h4 class="pharmacy-row__name">{{ stock.pharmacyName }}</h4>
        <p class="pharmacy-row__address">{{ stock.pharmacyAddress }}</p>
      </div>
    </div>

    <div class="pharmacy-row__price-block">
      <div class="pharmacy-row__price">{{ stock.price.toFixed(2) }} р.</div>
    </div>

    <div class="pharmacy-row__actions">
      <button class="btn-book">Забронировать</button>
      <a :href="'tel:' + stock.phone" class="btn-phone"></a>
    </div>
  </div>
</template>

<script setup>
defineProps({
  stock: Object 
});
</script>

<style scoped>
.pharmacy-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #f8f9ff;
  border-radius: 12px;
  padding: 12px 20px;
  margin-bottom: 10px;
  border: 1px solid #eee;
}

.pharmacy-row__info { display: flex; align-items: center; gap: 15px; flex: 2; }
.pharmacy-row__img { width: 50px; height: 50px; border-radius: 8px; object-fit: cover; }
.pharmacy-row__name { margin: 0; font-size: 14px; font-weight: bold; }
.pharmacy-row__address { margin: 2px 0; font-size: 13px; color: #555; }
.pharmacy-row__status { font-size: 12px; color: #00a65a; }

.pharmacy-row__price-block { flex: 1; text-align: center; }
.pharmacy-row__price { font-size: 18px; font-weight: 800; color: #333; }
.pharmacy-row__badge { display: inline-block; background: #eee; font-size: 11px; padding: 2px 8px; border-radius: 4px; margin-top: 4px; }
.pharmacy-row__time { font-size: 11px; color: #999; margin-top: 4px; }

.pharmacy-row__actions { display: flex; gap: 10px; align-items: center; }
.btn-book { background: #ffcc00; border: none; padding: 10px 20px; border-radius: 20px; font-weight: bold; cursor: pointer; }
.btn-phone { width: 40px; height: 40px; border-radius: 50%; border: 1px solid #eee; display: flex; align-items: center; justify-content: center; text-decoration: none; background: white; }
</style>