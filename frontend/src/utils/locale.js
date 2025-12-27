export function levelToRu(level) {
  if (level === 1 || level === 'Total') {
    return 'Итого';
  }
  if (level === 2 || level === 'Sku') {
    return 'Группа';
  }
  if (level === 3 || level === 'SkuSub') {
    return 'Подгруппа';
  }
  return String(level ?? '');
}

export function valueTypeToRu(valueType) {
  if (valueType === 1 || valueType === 'Units') {
    return 'Шт';
  }
  if (valueType === 2 || valueType === 'Price') {
    return 'Цена';
  }
  if (valueType === 3 || valueType === 'Amount') {
    return 'Сумма';
  }
  return String(valueType ?? '');
}

export function columnTitleRu(key, fallbackTitle) {
  const map = {
    level: 'Уровень',
    valueType: 'Показатель',
    skuName: 'Группа (SKU)',
    skuSubName: 'Подгруппа (SKU Sub)',
    historyY0: 'История Y0',
    planningY1: 'План Y1',
    contributionGrowth: 'Рост (вклад)',
  };

  return map[key] || fallbackTitle || key;
}
