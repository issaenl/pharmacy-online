<template>
  <div class="review-bubble">
    <div class="bubble-header">
      <div class="bubble-user">
        <div class="avatar">{{ review.userName.charAt(0) }}</div>
        <div class="user-meta">
          <div class="author-name">{{ review.userName }}</div>
          <div class="bubble-stars">
            <span v-for="n in 5" :key="n" :class="{'filled': n <= review.rating}">★</span>
          </div>
        </div>
      </div>
      <div class="review-date">{{ formattedDate }}</div>
    </div>
    <p class="bubble-text" v-if="review.comment">{{ review.comment }}</p>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  review: {
    type: Object,
    required: true
  }
});

const formattedDate = computed(() => {
  const options = { day: '2-digit', month: 'long', year: 'numeric' };
  return new Date(props.review.createdAt).toLocaleDateString('ru-RU', options);
});
</script>

<style scoped>
.review-bubble {
  background: #FAFAFA;
  border-radius: 20px;
  border-bottom-left-radius: 4px;
  padding: 20px;
  border: 1px solid #E8F4EA;
  display: flex;
  flex-direction: column;
  gap: 12px;
  height: 100%;
}

.bubble-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.bubble-user {
  display: flex;
  align-items: center;
  gap: 10px;
}

.avatar {
  width: 40px; 
  height: 40px;
  border-radius: 50%;
  background: #E8F4EA;
  color: var(--primary-color, #689D6D);
  display: flex; align-items: center; justify-content: center;
  font-weight: bold;
  font-size: 18px;
}

.user-meta { display: flex; flex-direction: column; }
.author-name { font-weight: 600; font-size: 15px; color: #333; }
.bubble-stars { color: #e5e7eb; font-size: 14px; letter-spacing: 1px; line-height: 1; margin-top: 2px;}
.bubble-stars .filled { color: var(--primary-color, #689D6D); } 

.review-date { font-size: 13px; color: #aaa; }
.bubble-text { font-size: 15px; color: #444; line-height: 1.5; margin: 0; font-style: italic; }
</style>