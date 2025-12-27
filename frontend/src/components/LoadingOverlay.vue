<template>
  <div v-if="show" class="overlay" data-cy="loadingOverlay">
    <div class="box">
      <div class="spinner" aria-hidden="true"></div>
      <div class="text">{{ text }}</div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  show: { type: Boolean, required: true },
  text: { type: String, required: false, default: 'Загрузка…' },
});
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
}

.box {
  background: #0b0e13;
  border: 1px solid #273041;
  border-radius: 14px;
  padding: 14px 16px;
  color: #e8eef7;
  display: flex;
  align-items: center;
  gap: 12px;
  box-shadow: 0 1px 18px rgba(0, 0, 0, 0.4);
}

.spinner {
  width: 18px;
  height: 18px;
  border-radius: 999px;
  border: 2px solid rgba(255, 255, 255, 0.25);
  border-top-color: rgba(255, 255, 255, 0.9);
  animation: spin 0.75s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.text {
  font-weight: 700;
}
</style>
