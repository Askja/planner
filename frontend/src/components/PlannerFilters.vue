<template>
  <div class="filters" data-cy="filters">
    <label class="label">
      Подгруппы (через запятую)
      <input
        data-cy="skuSubNameInput"
        class="input"
        type="text"
        :value="skuSubNameInput"
        :disabled="loading"
        placeholder="Кола 0.5л, Вода 1.5л"
        @input="$emit('update:skuSubNameInput', $event.target.value)"
      />
    </label>

    <div class="levels">
      <label class="check">
        <input
          data-cy="levelTotal"
          type="checkbox"
          :disabled="loading"
          :checked="selectedLevels.includes('Total')"
          @change="toggle('Total')"
        />
        Итого
      </label>

      <label class="check">
        <input
          data-cy="levelSku"
          type="checkbox"
          :disabled="loading"
          :checked="selectedLevels.includes('Sku')"
          @change="toggle('Sku')"
        />
        Группа
      </label>

      <label class="check">
        <input
          data-cy="levelSkuSub"
          type="checkbox"
          :disabled="loading"
          :checked="selectedLevels.includes('SkuSub')"
          @change="toggle('SkuSub')"
        />
        Подгруппа
      </label>

      <button data-cy="applyFilters" class="btn" :disabled="loading" @click="$emit('apply')">
        {{ loading ? 'Загрузка…' : 'Применить' }}
      </button>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  skuSubNameInput: { type: String, required: true },
  selectedLevels: { type: Array, required: true },
  loading: { type: Boolean, required: true },
});

const emit = defineEmits(['update:skuSubNameInput', 'update:selectedLevels', 'apply']);

function toggle(level) {
  const set = new Set(props.selectedLevels);
  if (set.has(level)) {
    set.delete(level);
  } else {
    set.add(level);
  }
  emit('update:selectedLevels', Array.from(set));
}
</script>

<style scoped>
.filters {
  display: flex;
  gap: 12px;
  align-items: flex-end;
  flex-wrap: wrap;
  padding: 12px;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  background: #ffffff;
}

.label {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 360px;
  color: #111827;
  font-weight: 600;
}

.input {
  padding: 8px 10px;
  border: 1px solid #d1d5db;
  border-radius: 10px;
  background: #ffffff;
  color: #111827;
}

.levels {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.check {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: #111827;
  font-weight: 600;
}

.btn {
  padding: 8px 12px;
  border-radius: 10px;
  border: 1px solid #d1d5db;
  background: #111827;
  color: #ffffff;
  cursor: pointer;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
