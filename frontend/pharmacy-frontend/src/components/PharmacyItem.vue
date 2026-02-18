<template>
  <div class="pharmacy-item">
    <div class="pharmacy-main-info">
      <div class="pharmacy-name">{{ pharmacy.pharmacyName }}</div>
      <div class="update-time">Последнее обновление: {{ formatDate(pharmacy.lastUpdate) }}</div>
    </div>
    
    <div class="pharmacy-address-col">
      <div class="pharmacy-address">{{ pharmacy.pharmacyAddress }}</div>
    </div>
    
    <div class="pharmacy-pricing">
      <div class="pharmacy-price">{{ formatPrice(pharmacy.price) }} р.</div>
      <div :class="['pharmacy-quantity', { 'low-stock': pharmacy.quantity < 10 }]">
        {{ pharmacy.quantity }} шт.
      </div>
    </div>

    <div class="pharmacy-actions">
      <button class="book-btn">Забронировать</button>
      <button class="call-btn" :title="pharmacy.pharmacyPhone">
        <img src="/assets/Phone.svg" alt="Позвонить" class="phone-icon">
      </button>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  pharmacy: {
    type: Object,
    required: true
  }
});

const formatPrice = (price) => {
  if (price == null) return '0.00';
  return Number(price).toFixed(2);
};

const formatDate = (dateString) => {
    if (!dateString) return 'н/д';
  
    const date = new Date(dateString);

   const time = date.toLocaleTimeString('ru-RU', {
        hour: '2-digit',
        minute: '2-digit',
    });

    const dayMonthYear = date.toLocaleDateString('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
    });

    return `${time}, ${dayMonthYear}`;
};
</script>

<style scoped>
    .pharmacy-item {
        display: grid;
        grid-template-columns: 2fr 1.5fr 1fr 240px;
        align-items: center;
        padding: 15px 25px;
        background: #fff;
        border-radius: 30px;
        margin-bottom: 10px;
    }

    .pharmacy-item:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.05);
    }

    .pharmacy-main-info {
        display: flex;
        flex-direction: column;
        gap: 4px;
    }

    .pharmacy-address { 
        font-weight: 600; 
        font-size: 16px;
        line-height: 1.2;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;  
        overflow: hidden;
    }

    .pharmacy-name { 
        font-size: 20px; 
        color: #000; 
    }

    .pharmacy-pricing {
        display: flex;
        flex-direction: column;
        align-items: center;
    }

    .pharmacy-price { 
        font-weight: 600; 
        font-size: 20px; 
        text-align: center; 
    }

    .pharmacy-quantity { 
        text-align: center;
        color: #B4AFAC;
    }

    .update-time { 
        font-size: 12px; 
        color: #B4AFAC; 
    }

    .pharmacy-actions { 
        display: flex; 
        gap: 10px; 
        justify-content: flex-end; 
        align-items: center;
    
    }

    .book-btn {
        background: #BB4E58;
        color: white;
        border: none;
        padding: 8px 20px;
        border-radius: 15px;
        cursor: pointer;
        font-weight: 500;
        font-size: 20px;
        font-family: var(--main-font);
        white-space: nowrap;
    }

    .call-btn {
        background: #BB4E58;
        border: none;
        border-radius: 50%;
        width: 40px;
        height: 40px;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        flex-shrink: 0;
    }

    .pharmacy-quantity.low-stock {
        color: #BB4E58; 
        font-weight: 700;
    }

    .phone-icon {
        width: 24px; 
        height: 24px;
    }

    @media (max-width: 1024px) {
        .pharmacy-item {
            grid-template-columns: 1.5fr 1fr 0.8fr 200px;
            padding: 15px;
            border-radius: 20px;
        }

        .pharmacy-name { 
            font-size: 16px; 
        }

        .book-btn { 
            font-size: 14px; 
            padding: 10px 15px; 
        }
    }

    @media (max-width: 768px) {
        .pharmacy-item {
            grid-template-columns: 1fr 1fr; 
            grid-template-areas: 
                "info pricing"
                "address actions";
            gap: 10px;
            padding: 20px;
        }

        .pharmacy-main-info { 
            grid-area: info; 
        }

        .pharmacy-pricing {
            grid-area: pricing; 
            align-items: flex-end; 
        }

        .pharmacy-address-col { 
            grid-area: address; 
        }

        .pharmacy-actions { 
            grid-area: actions; 
            margin-top: 5px; 
        }

        .pharmacy-price { 
            font-size: 18px; 
        }

        .pharmacy-address { 
            -webkit-line-clamp: 1; 
            min-height: auto; 
        }
    }

    @media (max-width: 480px) {
        .pharmacy-item {
            grid-template-columns: 1fr; 
            grid-template-areas: none;
            display: flex;
            flex-direction: column;
            align-items: flex-start;
            border-radius: 15px;
        }

        .pharmacy-main-info, .pharmacy-address-col, .pharmacy-pricing, .pharmacy-actions {
            width: 100%;
            text-align: left;
        }

        .pharmacy-pricing {
            flex-direction: row;
            justify-content: space-between;
            align-items: center;
            margin: 10px 0;
            padding: 10px 0;
            border-top: 1px solid #f5f5f5;
            border-bottom: 1px solid #f5f5f5;
        }

        .pharmacy-actions {
            justify-content: space-between;
        }

        .book-btn {
            flex-grow: 1;
            text-align: center;
        }
    }
</style>