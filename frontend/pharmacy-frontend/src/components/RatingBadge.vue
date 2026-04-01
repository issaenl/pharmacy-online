<template>
  <span class="rating-badge" :class="badgeClass">
    ★ {{ rating }}
  </span>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  rating: {
    type: Number,
    required: true
  }
});

const badgeClass = computed(() => {
  if (props.rating >= 4.5) return 'rating-5';
  if (props.rating >= 4.0) return 'rating-4';
  if (props.rating >= 3.0) return 'rating-3'; 
  return 'rating-bad';                        
});
</script>

<style scoped>
.rating-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  padding: 4px 8px;
  border-radius: 12px;
  font-weight: bold;
  font-size: 14px;
  white-space: nowrap;
}

.rating-5 { color: #689D6D; background: #E8F4EA; } 
.rating-4 { color: #84A950; background: #F1F7E8; }
.rating-3 { color: #D97706; background: #FFF8E1; } 
.rating-bad { color: #BB4E58; background: #FDE8E8; } 
</style>