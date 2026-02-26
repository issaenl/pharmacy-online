<template>
  <div class="modal-overlay" v-if="isOpen" @click.self="closeModal">
    <div class="modal-content">
      <div class="modal-header">
        <h2>Выберите аптеку</h2>
        <button @click="closeModal" class="close-btn">×</button>
      </div>
      
      <div v-if="pharmacies.length === 0 && !isLoadingScript" class="no-pharmacies-msg">
        К сожалению, нет ни одной аптеки, в которой весь ваш заказ был бы в наличии.
      </div>

      <div id="yandex-map" class="map-container" v-show="pharmacies.length > 0">
        <div v-if="isLoadingScript" class="loading-overlay">Загрузка карты...</div>
      </div>
      
      <div v-if="selectedPharmacy" class="selected-info">
        <p><strong>Выбрана аптека:</strong> {{ selectedPharmacy.name }}</p>
        <p>{{ selectedPharmacy.address }}, {{ selectedPharmacy.district }}</p>
        <button class="confirm-btn" @click="confirmSelection">Подтвердить выбор</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, nextTick } from 'vue';
import api from '@/api/api';
import { useCartStore } from '@/stores/cartStore';

const props = defineProps({
  isOpen: Boolean
});

const emit = defineEmits(['close', 'select']);

const cartStore = useCartStore();
const pharmacies = ref([]);
const selectedPharmacy = ref(null);
const isLoadingScript = ref(false);
let myMap = null;

const loadYandexMapScript = () => {
  return new Promise((resolve, reject) => {
    if (window.ymaps) {
      resolve(window.ymaps);
      return;
    }

    isLoadingScript.value = true;
    
    const apiKey = import.meta.env.VITE_YANDEX_MAP_API_KEY; 
    const script = document.createElement('script');

    const url = apiKey 
      ? `https://api-maps.yandex.ru/2.1/?apikey=${apiKey}&lang=ru_RU`
      : `https://api-maps.yandex.ru/2.1/?lang=ru_RU`;

    script.src = url; 
    script.type = "text/javascript";
    
    script.onload = () => {
      isLoadingScript.value = false;
      resolve(window.ymaps);
    };
    script.onerror = (error) => {
      isLoadingScript.value = false;
      reject(error);
    };
    
    document.head.appendChild(script);
  });
};

const fetchAvailablePharmacies = async () => {
  try {
    const requestData = cartStore.items.map(item => ({
      productId: item.productId,
      quantity: item.quantity
    }));

    const response = await api.post('/Pharmacies/available-for-cart', requestData);
    pharmacies.value = response.data;
  } catch (error) {
    console.error("Ошибка загрузки подходящих аптек:", error);
    pharmacies.value = [];
  }
};

const initMap = () => {
  if (myMap) {
    myMap.destroy();
    myMap = null;
  }

  if (pharmacies.value.length === 0) return;

  const centerCoords = pharmacies.value[0]?.latitude && pharmacies.value[0]?.longitude 
    ? [pharmacies.value[0].latitude, pharmacies.value[0].longitude] 
    : [53.9006, 27.5590]; 

  myMap = new window.ymaps.Map("yandex-map", {
    center: centerCoords, 
    zoom: 11,
    controls: ['zoomControl', 'fullscreenControl']
  });

  pharmacies.value.forEach(pharmacy => {
    if (pharmacy.latitude && pharmacy.longitude) {
      const placemark = new window.ymaps.Placemark([pharmacy.latitude, pharmacy.longitude], {
        hintContent: pharmacy.name,
        balloonContentHeader: pharmacy.name,
        balloonContentBody: `${pharmacy.address}<br>Телефон: ${pharmacy.phone}`
      }, {
        preset: 'islands#redMedicalIcon' 
      });

      placemark.events.add('click', () => {
        selectedPharmacy.value = pharmacy;
      });

      myMap.geoObjects.add(placemark);
    }
  });
};

watch(() => props.isOpen, async (newVal) => {
  if (newVal) {
    selectedPharmacy.value = null; 
    
    await fetchAvailablePharmacies();
    await nextTick();

    if (pharmacies.value.length > 0) {
      try {
        const ymaps = await loadYandexMapScript();
        ymaps.ready(initMap);
      } catch (e) {
        console.error("Не удалось загрузить карту", e);
      }
    }
  } else {
    if (myMap) {
      myMap.destroy();
      myMap = null;
    }
  }
});

const closeModal = () => emit('close');
const confirmSelection = () => {
  emit('select', selectedPharmacy.value);
  closeModal();
};
</script>

<style scoped>
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 2000;
    }

    .modal-content {
      background: white;
      border-radius: 20px;
      width: 90%;
      max-width: 800px;
      overflow: hidden;
      display: flex;
      flex-direction: column;
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px;
      background: #ffffff;
    }

    .modal-header h3 {
      margin: 0;
      color: #000;
      font-family: var(--main-font);
    }

    .close-btn {
      background: none;
      border: none;
      font-size: 24px;
      cursor: pointer;
      color: #888;
    }

    .close-btn:hover {
      color: var(--accent-color);
    }

    .no-pharmacies-msg {
      padding: 40px 20px;
      text-align: center;
      color: var(--accent-color);
      font-weight: 600;
      font-size: 20px;
      font-family: var(--main-font);
    }

    .map-container {
      width: 100%;
      height: 500px;
      background: #fff;
      position: relative;
    }

    .loading-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      background: rgba(255, 255, 255, 0.8);
      font-family: var(--main-font);
      font-weight: 600;
      z-index: 10;
    }

    .selected-info {
      padding: 20px;
      background: white;
      display: flex;
      flex-direction: column;
      gap: 10px;
      border-top: 1px solid #eee;
      font-family: var(--main-font);
    }

    .selected-info p {
      margin: 0;
      font-size: 16px;
      color: #000;
    }

    .confirm-btn {
      background: var(--primary-color);
      color: white;
      border: none;
      padding: 12px;
      border-radius: 10px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      align-self: flex-start;
      font-family: var(--main-font);
    }

    .confirm-btn:hover {
      background: var(--primary-color);
    }

    @media (max-width: 600px) {
      .map-container {
        height: 350px;
      }
    }
</style>