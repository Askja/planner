export function formatNumber(value, { decimals = 2 } = {}) {
  const n = Number(value);
  if (!Number.isFinite(n)) {
    return '';
  }
  return new Intl.NumberFormat(undefined, {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(n);
}

export function formatInteger(value) {
  const n = Number(value);
  if (!Number.isFinite(n)) {
    return '';
  }
  return new Intl.NumberFormat(undefined, {
    maximumFractionDigits: 0,
  }).format(n);
}

export function formatPercent(value, { decimals = 2 } = {}) {
  const n = Number(value);
  if (!Number.isFinite(n)) {
    return '';
  }
  return new Intl.NumberFormat(undefined, {
    style: 'percent',
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(n);
}

export function safeTrimArray(arr) {
  if (!Array.isArray(arr)) {
    return [];
  }
  return arr.map((x) => String(x ?? '').trim()).filter((x) => x.length > 0);
}
