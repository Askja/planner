import { defineStore } from 'pinia';

let nextId = 1;

export const useNotificationsStore = defineStore('notificationsStore', {
  state: () => ({
    items: [],
  }),

  actions: {
    push({ type = 'info', title = '', message = '', timeoutMs = 5000 }) {
      const id = nextId++;
      this.items.push({ id, type, title, message });

      if (timeoutMs > 0) {
        setTimeout(() => this.remove(id), timeoutMs);
      }

      return id;
    },

    error(message, title = 'Ошибка') {
      return this.push({ type: 'error', title, message, timeoutMs: 8000 });
    },

    info(message, title = 'Инфо') {
      return this.push({ type: 'info', title, message, timeoutMs: 4000 });
    },

    remove(id) {
      this.items = this.items.filter((x) => x.id !== id);
    },

    clear() {
      this.items = [];
    },
  },
});
