<template>
  <PlannerFilters
    :skuSubNameInput="store.skuSubNameInput"
    :selectedLevels="store.selectedLevels"
    :loading="store.loading"
    @update:skuSubNameInput="(v) => (store.skuSubNameInput = v)"
    @update:selectedLevels="(v) => (store.selectedLevels = v)"
    @apply="onApply"
  />

  <div class="spacer"></div>

  <PlannerTable
    :rows="store.data"
    :columns="store.columns"
    :savingBySkuSubId="store.savingBySkuSubId"
    @editPlanningUnits="onEditPlanningUnits"
  />
</template>

<script setup>
import { onMounted } from 'vue';
import PlannerFilters from '@/components/PlannerFilters.vue';
import PlannerTable from '@/components/PlannerTable.vue';
import { usePlannerStore } from '@/stores/plannerStore';

const store = usePlannerStore();

async function onApply() {
  await store.fetchPlanner();
}

async function onEditPlanningUnits(payload) {
  try {
    await store.updatePlanningUnits(payload);
  } catch (e) {
    console.error(e);
  }
}

onMounted(async () => {
  await store.fetchPlanner();
});
</script>

<style scoped>
.spacer {
  height: 12px;
}
</style>
