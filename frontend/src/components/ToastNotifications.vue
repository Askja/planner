<template>
  <div class="toasts" aria-live="polite" aria-relevant="additions" data-cy="toasts">
    <div v-for="t in store.items" :key="t.id" class="toast" :class="t.type">
      <div class="head">
        <div class="title">{{ t.title }}</div>
        <button class="close" @click="store.remove(t.id)" aria-label="Закрыть">✕</button>
      </div>
      <div class="msg">{{ t.message }}</div>
    </div>
  </div>
</template>

<script setup>
import { useNotificationsStore } from '@/stores/notificationStore';
const store = useNotificationsStore();
</script>

<style scoped>
.toasts {
  position: fixed;
  top: 14px;
  right: 14px;
  z-index: 60;
  display: flex;
  flex-direction: column;
  gap: 10px;
  width: 360px;
  max-width: calc(100vw - 28px);
}

.toast {
  border-radius: 14px;
  border: 1px solid #273041;
  background: #0b0e13;
  color: #e8eef7;
  padding: 10px 12px;
  box-shadow: 0 1px 18px rgba(0, 0, 0, 0.35);
}

.toast.error {
  border-color: rgba(255, 90, 90, 0.5);
}

.toast.info {
  border-color: rgba(120, 170, 255, 0.5);
}

.head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}

.title {
  font-weight: 900;
}

.msg {
  margin-top: 6px;
  color: #cfd7e6;
  line-height: 1.35;
}

.close {
  border: 0;
  background: transparent;
  color: #cfd7e6;
  cursor: pointer;
  font-size: 16px;
  padding: 2px 6px;
  border-radius: 8px;
}

.close:hover {
  background: rgba(255, 255, 255, 0.06);
}
</style>
