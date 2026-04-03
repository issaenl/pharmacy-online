<template>
  <div class="pharmacy-item" @click="goToPharmacy">
    <div class="pharmacy-main-info">
      <div class="pharmacy-name">{{ pharmacy.name }}</div>
      
      <div class="rating-wrapper">
        <RatingBadge v-if="pharmacy.rating" :rating="pharmacy.rating" />
        <span v-else class="no-rating">Нет оценок</span>
      </div>
    </div>
    
    <div class="pharmacy-address-col">
      <div class="pharmacy-address">{{ pharmacy.address }}</div>
      <div class="pharmacy-district">{{ pharmacy.district }}</div>
    </div>
    
    <div class="pharmacy-actions">
        <button class="details-btn">Подробнее</button>

        <a 
            v-if="pharmacy.phone" 
            :href="`tel:${pharmacy.phone}`" 
            class="call-btn" 
            :title="pharmacy.phone" 
            @click.stop>
            <img src="/assets/Phone.svg" alt="Позвонить" class="phone-icon">
        </a>
      
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';
import RatingBadge from '@/components/RatingBadge.vue';

const props = defineProps({
  pharmacy: {
    type: Object,
    required: true
  }
});

const router = useRouter();

const goToPharmacy = () => {
  const id = props.pharmacy.id || props.pharmacy.pharmacyId;
  if (id) router.push(`/pharmacy/${id}`);
};
</script>

<style scoped>
    .pharmacy-item {
        display: grid;
        grid-template-columns: 2fr 2fr minmax(200px, auto);
        align-items: center;
        padding: 15px 25px;
        background: #fff;
        border-radius: 30px;
        margin-bottom: 10px;
        cursor: pointer;
        transition: 0.2s;
    }

    .pharmacy-item:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.05);
    }

    .pharmacy-main-info { display: flex; flex-direction: column; gap: 8px; }
    .pharmacy-name { font-size: 20px; color: #000; font-weight: 600; }
    
    .rating-wrapper { display: flex; align-items: center; }
    .no-rating { font-size: 13px; color: #999; }

    .pharmacy-address-col { display: flex; flex-direction: column; gap: 6px; align-items: flex-start; }
    .pharmacy-address { font-weight: 600; font-size: 16px; line-height: 1.2; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
    .pharmacy-district { font-size: 14px; color: #888; }

    .pharmacy-actions { display: flex; gap: 10px; justify-content: flex-end; align-items: center; }
    
    .details-btn { 
        background: transparent; 
        background: #BB4E58; 
        color: white;
        border: none;
        padding: 8px 20px; 
        border-radius: 15px; 
        cursor: pointer; 
        font-weight: 600; 
        font-size: 16px; 
        font-family: var(--main-font); 
        white-space: nowrap; 
        transition: 0.2s;
    }

    .details-btn:hover{
        color: #BB4E58;
        background-color: #fff;
        border: 2px solid #BB4E58;
    }

    .call-btn { background: #BB4E58; border: none; border-radius: 50%; width: 40px; height: 40px; display: flex; align-items: center; justify-content: center; cursor: pointer; flex-shrink: 0; }
    .phone-icon { width: 24px; height: 24px; }

    @media (max-width: 1024px) {
        .pharmacy-item { grid-template-columns: 1.5fr 1fr minmax(180px, auto); padding: 15px; border-radius: 20px; }
        .pharmacy-name { font-size: 16px; }
        .details-btn { font-size: 14px; padding: 10px 15px; }
    }

    @media (max-width: 768px) {
        .pharmacy-item { 
            grid-template-columns: 1fr; 
            grid-template-areas: "info" "address" "actions"; 
            gap: 15px; 
            padding: 20px; 
        }
        .pharmacy-main-info { grid-area: info; }
        .pharmacy-address-col { grid-area: address; }
        .pharmacy-actions { grid-area: actions; justify-content: flex-start; margin-top: 5px; }
        .pharmacy-address { -webkit-line-clamp: 1; min-height: auto; }
    }

    @media (max-width: 480px) {
        .pharmacy-item { border-radius: 15px; }
        .pharmacy-actions { width: 100%; justify-content: space-between; }
        .details-btn { flex-grow: 1; text-align: center; }
    }
</style>