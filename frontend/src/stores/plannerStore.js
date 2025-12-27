import { defineStore } from 'pinia';
import { getPlanner, patchPlanner } from '../api/plannerApi';
import { safeTrimArray } from '../utils/formatters';
import { columnTitleRu } from '@/utils/locale.js';

function normalizeLevels(levels) {
  if (!Array.isArray(levels)) {
    return [];
  }
  return levels.filter((x) => x === 'Total' || x === 'Sku' || x === 'SkuSub');
}

export const usePlannerStore = defineStore('plannerStore', {
  state: () => ({
    data: [],
    metadata: [],
    loading: false,
    error: null,

    skuSubNameInput: '',
    selectedLevels: ['Total', 'Sku', 'SkuSub'],

    savingBySkuSubId: {},

    lastQuery: {
      skuSubNames: [],
      levels: ['Total', 'Sku', 'SkuSub'],
    },
  }),

  getters: {
    columns(state) {
      const fallback = [
        { key: 'level', title: 'Уровень', dataType: 'enum', isEditable: false },
        { key: 'valueType', title: 'Показатель', dataType: 'enum', isEditable: false },
        { key: 'skuName', title: 'Группа (SKU)', dataType: 'string', isEditable: false },
        { key: 'skuSubName', title: 'Подгруппа (SKU Sub)', dataType: 'string', isEditable: false },
        { key: 'historyY0', title: 'История Y0', dataType: 'decimal', isEditable: false },
        { key: 'planningY1', title: 'План Y1', dataType: 'decimal', isEditable: true },
        { key: 'contributionGrowth', title: 'Рост (вклад)', dataType: 'decimal', isEditable: false },
      ];

      const meta = Array.isArray(state.metadata) && state.metadata.length > 0 ? state.metadata : fallback;
      return meta.map((m) => ({
        key: m.key,
        title: columnTitleRu(m.title),
        dataType: m.dataType,
        style: m.style || null,
        isEditable: Boolean(m.isEditable),
      }));
    },

    skuSubNames(state) {
      return safeTrimArray(state.skuSubNameInput.split(','));
    },
  },

  actions: {
    async fetchPlanner({ skuSubNames, levels } = {}) {
      const finalSkuSubNames = Array.isArray(skuSubNames) ? skuSubNames : this.skuSubNames;
      const finalLevels = normalizeLevels(levels || this.selectedLevels);

      this.loading = true;
      this.error = null;

      try {
        const res = await getPlanner({
          skuSubNames: finalSkuSubNames,
          levels: finalLevels,
        });

        this.data = Array.isArray(res?.data) ? res.data : [];
        this.metadata = Array.isArray(res?.metadata) ? res.metadata : [];

        this.lastQuery = {
          skuSubNames: finalSkuSubNames,
          levels: finalLevels,
        };
      } catch (e) {
        this.error = e;
      } finally {
        this.loading = false;
      }
    },

    async updatePlanningUnits({ skuSubId, planningUnits }) {
      const id = Number(skuSubId);
      const units = Number(planningUnits);

      if (!Number.isFinite(id) || id <= 0) {
        throw new Error('Invalid skuSubId');
      }
      if (!Number.isFinite(units) || units < 0) {
        throw new Error('Invalid planningUnits');
      }

      this.savingBySkuSubId[id] = true;
      this.error = null;

      try {
        await patchPlanner({
          skuSubId: id,
          planningUnits: units,
        });

        await this.fetchPlanner({
          skuSubNames: this.lastQuery.skuSubNames,
          levels: this.lastQuery.levels,
        });
      } catch (e) {
        this.error = e;
        throw e;
      } finally {
        this.savingBySkuSubId[id] = false;
      }
    },
  },
});
