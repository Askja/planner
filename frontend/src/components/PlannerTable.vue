<template>
  <div class="tableBox">
    <table class="table" data-cy="plannerTable">
      <thead>
        <tr>
          <th v-for="col in orderedColumns" :key="col.key" :class="thClass(col)">
            {{ col.title }}
          </th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="(row, idx) in displayRows" :key="rowKey(row, idx)" :class="rowClass(idx, row)">
          <template v-for="col in orderedColumns" :key="col.key">
            <td v-if="col.key === 'skuName' && row.skuRowSpan > 0" class="tdLeft tdSku" :rowspan="row.skuRowSpan">
              {{ row.skuCellText }}
            </td>

            <td
              v-else-if="col.key === 'skuSubName' && row.skuSubRowSpan > 0"
              class="tdLeft tdSub"
              :rowspan="row.skuSubRowSpan"
            >
              {{ row.skuSubCellText }}
            </td>

            <template v-else-if="col.key === 'skuName' || col.key === 'skuSubName'"> </template>

            <td v-else :class="tdClass(col)">
              <template v-if="col.key === 'valueType'">
                <span class="valueType">{{ valueTypeToRu(row.valueTypeNorm) }}</span>
              </template>

              <template v-else-if="col.key === 'planningY1' && isPlanningEditable(row, col)">
                <InlineNumberCell
                  :value="row.planningY1"
                  :disabled="Boolean(savingBySkuSubId[row.skuSubIdNorm])"
                  :dataCy="`editPlanning-${row.skuSubIdNorm}`"
                  @commit="(val) => commitPlanning(row, val)"
                />
                <span v-if="savingBySkuSubId[row.skuSubIdNorm]" class="saving">сохраняю…</span>
              </template>

              <template v-else-if="col.key === 'contributionGrowth'">
                <span v-if="row.contributionGrowth === null || row.contributionGrowth === undefined">—</span>
                <span v-else>{{ formatPercentSafe(row.contributionGrowth) }}</span>
              </template>

              <template v-else>
                {{ renderCell(row, col) }}
              </template>
            </td>
          </template>
        </tr>

        <tr v-if="displayRows.length === 0">
          <td class="empty" :colspan="orderedColumns.length" data-cy="empty">Нет данных</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import InlineNumberCell from '@/components/InlineNumberCell.vue';
import { formatNumber, formatPercent } from '@/utils/formatters';

const props = defineProps({
  rows: { type: Array, required: true },
  columns: { type: Array, required: true },
  savingBySkuSubId: { type: Object, required: true },
});

const emit = defineEmits(['editPlanningUnits']);

const preferredOrder = [
  'skuName',
  'skuSubName',
  'valueType',
  'historyY0',
  'planningY1',
  'contributionGrowth',
];
const valueTypeOrder = { Units: 0, Price: 1, Amount: 2 };

function normalizeLevel(level) {
  if (level === 'TOTAL' || level === 'Total' || level === 1) {
    return 'Total';
  }

  if (level === 'SKU' || level === 'Sku' || level === 2) {
    return 'Sku';
  }

  if (level === 'SKUSUB' || level === 'SkuSub' || level === 3) {
    return 'SkuSub';
  }

  return String(level ?? '');
}

function normalizeValueType(valueType) {
  if (valueType === 'UNITS' || valueType === 'Units' || valueType === 1) {
    return 'Units';
  }

  if (valueType === 'PRICE' || valueType === 'Price' || valueType === 2) {
    return 'Price';
  }

  if (valueType === 'AMOUNT' || valueType === 'Amount' || valueType === 3) {
    return 'Amount';
  }

  return String(valueType ?? '');
}

function valueTypeToRu(vtNorm) {
  if (vtNorm === 'Units') {
    return 'Количество';
  }

  if (vtNorm === 'Price') {
    return 'Цена';
  }

  if (vtNorm === 'Amount') {
    return 'Сумма';
  }

  return vtNorm;
}

function getSkuName(row) {
  return row.skuName ?? row.SKUName ?? row.SkuName ?? '';
}

function getSkuSubName(row) {
  return row.skuSubName ?? row.SKUSubName ?? row.SkuSubName ?? '';
}

function getSkuId(row) {
  const v = row.skuId ?? row.SKUId ?? row.SkuId ?? 0;

  return Number(v) || 0;
}

function getSkuSubId(row) {
  const v = row.skuSubId ?? row.SKUSubId ?? row.SkuSubId ?? 0;

  return Number(v) || 0;
}

const orderedColumns = computed(() => {
  const cols = Array.isArray(props.columns) ? props.columns : [];
  const byKey = new Map(cols.map((c) => [c.key, c]));

  const ordered = [];
  for (const key of preferredOrder) {
    if (byKey.has(key)) {
      ordered.push(byKey.get(key));
    }
  }

  for (const c of cols) {
    if (!preferredOrder.includes(c.key)) {
      ordered.push(c);
    }
  }

  const ensure = (key, title) => {
    if (!ordered.some((x) => x.key === key)) {
      ordered.unshift({
        key,
        title,
        dataType: 'string',
        isEditable: false,
      });
    }
  };

  ensure('skuSubName', 'Подгруппа');
  ensure('skuName', 'Группа');

  const ruTitles = {
    skuName: 'Группа',
    skuSubName: 'Подгруппа',
    valueType: 'Тип значения',
    historyY0: 'История Y0',
    planningY1: 'План Y1',
    contributionGrowth: 'Вклад роста',
  };

  return ordered.map((c) => ({ ...c, title: ruTitles[c.key] || c.title || c.key }));
});

function thClass(col) {
  if (col.key === 'historyY0' || col.key === 'planningY1' || col.key === 'contributionGrowth') {
    return 'thRight';
  }

  return 'thLeft';
}

function tdClass(col) {
  if (col.key === 'historyY0' || col.key === 'planningY1' || col.key === 'contributionGrowth') {
    return 'tdRight';
  }

  return 'tdLeft';
}

function rowKey(row, idx) {
  return `${row.levelNorm}-${row.valueTypeNorm}-${row.skuIdNorm}-${row.skuSubIdNorm}-${idx}`;
}

function rowClass(idx, row) {
  const cls = [];

  if (idx % 2 === 1) {
    cls.push('zebra');
  }

  if (row.levelNorm === 'Sku') {
    cls.push('lvlSku');
  }

  if (row.levelNorm === 'Total') {
    cls.push('lvlTotal');
  }

  return cls.join(' ');
}

function formatNumberSafe(v) {
  const n = Number(v);

  if (!Number.isFinite(n)) {
    return '';
  }

  return formatNumber(n, { decimals: 2 });
}

function formatPercentSafe(v) {
  const n = Number(v);

  if (!Number.isFinite(n)) {
    return '';
  }

  return formatPercent(n, { decimals: 2 });
}

function renderCell(row, col) {
  const key = col.key;
  const val = row[key];

  if (key === 'historyY0' || key === 'planningY1') {
    return formatNumberSafe(val);
  }

  if (key === 'skuName') {
    return row.skuNameNorm;
  }

  if (key === 'skuSubName') {
    return row.skuSubNameNorm;
  }

  if (val === null || val === undefined) {
    return '';
  }

  return String(val);
}

function isPlanningEditable(row, col) {
  if (!col.isEditable) {
    return false;
  }

  if (row.levelNorm !== 'SkuSub') {
    return false;
  }

  if (row.valueTypeNorm !== 'Units') {
    return false;
  }

  return row.skuSubIdNorm > 0;
}

function commitPlanning(row, newValue) {
  emit('editPlanningUnits', { skuSubId: row.skuSubIdNorm, planningUnits: Number(newValue) });
}

const displayRows = computed(() => {
  const raw = Array.isArray(props.rows) ? props.rows : [];

  const normalized = raw.map((r) => {
    const skuIdNorm = getSkuId(r);
    const skuSubIdNorm = getSkuSubId(r);

    return {
      ...r,
      skuIdNorm,
      skuSubIdNorm,
      skuNameNorm: getSkuName(r),
      skuSubNameNorm: getSkuSubName(r),
      levelNorm: normalizeLevel(r.level),
      valueTypeNorm: normalizeValueType(r.valueType),
    };
  });

  const totalRows = normalized.filter((x) => x.levelNorm === 'Total');
  const nonTotal = normalized.filter((x) => x.levelNorm !== 'Total');

  const bySkuId = new Map();
  for (const r of nonTotal) {
    if (!bySkuId.has(r.skuIdNorm)) {
      bySkuId.set(r.skuIdNorm, []);
    }

    bySkuId.get(r.skuIdNorm).push(r);
  }

  const ordered = [];
  const skuIds = Array.from(bySkuId.keys()).sort((a, b) => a - b);

  for (const skuId of skuIds) {
    const group = bySkuId.get(skuId) || [];

    const subs = group.filter((x) => x.levelNorm === 'SkuSub');
    const skuTotals = group.filter((x) => x.levelNorm === 'Sku');

    const bySubId = new Map();
    for (const r of subs) {
      if (!bySubId.has(r.skuSubIdNorm)) {
        bySubId.set(r.skuSubIdNorm, []);
      }

      bySubId.get(r.skuSubIdNorm).push(r);
    }

    const subIds = Array.from(bySubId.keys()).sort((a, b) => a - b);
    for (const subId of subIds) {
      const rows = bySubId.get(subId) || [];
      rows.sort(
        (a, b) => (valueTypeOrder[a.valueTypeNorm] ?? 99) - (valueTypeOrder[b.valueTypeNorm] ?? 99),
      );
      ordered.push(...rows);
    }

    skuTotals.sort(
      (a, b) => (valueTypeOrder[a.valueTypeNorm] ?? 99) - (valueTypeOrder[b.valueTypeNorm] ?? 99),
    );
    ordered.push(...skuTotals);
  }

  totalRows.sort(
    (a, b) => (valueTypeOrder[a.valueTypeNorm] ?? 99) - (valueTypeOrder[b.valueTypeNorm] ?? 99),
  );
  ordered.push(...totalRows);

  const rowsWithCells = ordered.map((r) => {
    const isSkuTotal = r.levelNorm === 'Sku';
    const isTotal = r.levelNorm === 'Total';

    const skuSpanKey = isTotal ? 'total' : `sku-${r.skuIdNorm}`;
    const skuSubSpanKey = isTotal
      ? 'totalSub'
      : isSkuTotal
        ? `skuTotal-${r.skuIdNorm}`
        : `sub-${r.skuSubIdNorm}`;

    const skuCellText = isTotal ? 'Итого' : r.skuNameNorm;
    const skuSubCellText = isTotal ? 'Всего' : isSkuTotal ? 'Итого SKU' : r.skuSubNameNorm;

    return {
      ...r,
      skuSpanKey,
      skuSubSpanKey,
      skuCellText,
      skuSubCellText,
      skuRowSpan: 0,
      skuSubRowSpan: 0,
    };
  });

  for (let i = 0; i < rowsWithCells.length; ) {
    const key = rowsWithCells[i].skuSpanKey;
    let j = i + 1;

    while (j < rowsWithCells.length && rowsWithCells[j].skuSpanKey === key) {
      j += 1;
    }

    rowsWithCells[i].skuRowSpan = j - i;

    for (let k = i + 1; k < j; k++) {
      rowsWithCells[k].skuRowSpan = 0;
    }

    i = j;
  }

  for (let i = 0; i < rowsWithCells.length; ) {
    const key = rowsWithCells[i].skuSubSpanKey;
    let j = i + 1;

    while (j < rowsWithCells.length && rowsWithCells[j].skuSubSpanKey === key) {
      j += 1;
    }

    rowsWithCells[i].skuSubRowSpan = j - i;

    for (let k = i + 1; k < j; k++) {
      rowsWithCells[k].skuSubRowSpan = 0;
    }

    i = j;
  }

  return rowsWithCells;
});
</script>

<style scoped>
.tableBox {
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  overflow: auto;
  background: #ffffff;
}

.table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  min-width: 980px;
  color: #111827;
}

thead th {
  position: sticky;
  top: 0;
  z-index: 2;
  background: #f9fafb;
  border-bottom: 1px solid #e5e7eb;
  padding: 10px 12px;
  font-weight: 700;
  color: #111827;
}

tbody td {
  border-bottom: 1px solid #eef2f7;
  padding: 10px 12px;
  vertical-align: middle;
}

.thLeft,
.tdLeft {
  text-align: left;
}

.thRight,
.tdRight {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.tdSku {
  font-weight: 800;
  color: #111827;
}

.tdSub {
  font-weight: 700;
  color: #111827;
}

.valueType {
  font-weight: 800;
  color: #111827;
}

.zebra td {
  background: #fbfdff;
}

.lvlSku td {
  background: #f8fafc;
}

.lvlTotal td {
  font-weight: 800;
  background: #f3f4f6;
}

.saving {
  margin-left: 10px;
  font-size: 12px;
  color: #6b7280;
}

.empty {
  padding: 14px;
  text-align: center;
  color: #6b7280;
}
</style>
