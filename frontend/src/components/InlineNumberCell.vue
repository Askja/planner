<template>
  <input
    :data-cy="dataCy"
    class="cellInput"
    type="text"
    inputmode="decimal"
    :disabled="disabled"
    :value="draft"
    @focus="onFocus"
    @input="onInput"
    @keydown="onKeyDown"
    @blur="onBlur"
  />
</template>

<script setup>
import { ref, watch } from 'vue';

const props = defineProps({
  value: { type: [Number, String], required: true },
  disabled: { type: Boolean, required: false, default: false },
  dataCy: { type: String, required: false, default: '' },
});

const emit = defineEmits(['commit']);

const draft = ref(String(props.value ?? ''));
const beforeEdit = ref(String(props.value ?? ''));

watch(
  () => props.value,
  (v) => {
    const s = String(v ?? '');
    draft.value = s;
    beforeEdit.value = s;
  },
);

function normalizeNumber(text) {
  const s = String(text ?? '')
    .trim()
    .replace(',', '.');
  const n = Number(s);
  if (!Number.isFinite(n)) {
    return null;
  }
  return n;
}

function onFocus(e) {
  beforeEdit.value = draft.value;
  if (e && e.target && typeof e.target.select === 'function') {
    e.target.select();
  }
}

function onInput(e) {
  draft.value = e.target.value;
}

function commit() {
  const n = normalizeNumber(draft.value);
  if (n === null) {
    draft.value = beforeEdit.value;
    return;
  }
  emit('commit', n);
}

function onKeyDown(e) {
  if (e.key === 'Enter') {
    e.preventDefault();
    commit();
    e.target.blur();
  } else if (e.key === 'Escape') {
    e.preventDefault();
    draft.value = beforeEdit.value;
    e.target.blur();
  }
}

function onBlur() {
  commit();
}
</script>

<style scoped>
.cellInput {
  width: 110px;
  max-width: 100%;
  padding: 6px 8px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  background: #ffffff;
  color: #111827;
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.cellInput:focus {
  outline: 2px solid #93c5fd;
  outline-offset: 1px;
}

.cellInput:disabled {
  background: #f3f4f6;
  color: #6b7280;
}
</style>
