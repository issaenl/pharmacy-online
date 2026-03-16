<template>
  <div v-if="errors && errors.length > 0" class="import-errors-alert">
    <div class="error-header">
      <h4>Внимание! Некоторые строки не были загружены:</h4>
      <button class="btn-close-error" @click="$emit('clear')">✕</button>
    </div>
    <ul class="error-list">
      <li v-for="(error, index) in errors" :key="index">{{ error }}</li>
    </ul>
  </div>
</template>

<script setup>
defineProps({
  errors: {
    type: Array,
    default: () => []
  }
});

defineEmits(['clear']);
</script>