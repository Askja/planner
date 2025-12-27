import { httpClient } from './httpClient';

export async function getPlanner({ skuSubNames, levels } = {}) {
  return await httpClient.get('/api/planner', {
    query: {
      skuSubName: skuSubNames || [],
      level: levels || [],
    },
  });
}

export async function patchPlanner({ skuSubId, planningUnits }) {
  return await httpClient.patch('/api/planner', {
    body: {
      skuSubId,
      planningUnits,
    },
  });
}
