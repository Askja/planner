function createCorrelationId() {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) {
    return crypto.randomUUID();
  }

  return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

function getTimeoutMs() {
  const raw = import.meta.env.VITE_HTTP_TIMEOUT_MS;
  const n = Number(raw);
  return Number.isFinite(n) && n > 0 ? n : 15000;
}

function buildUrl(path, query) {
  const url = new URL(path, window.location.origin);
  if (query && typeof query === 'object') {
    Object.entries(query).forEach(([key, value]) => {
      if (value === undefined || value === null) {
        return;
      }

      if (Array.isArray(value)) {
        value.forEach((v) => {
          if (v !== undefined && v !== null && String(v).trim() !== '') {
            url.searchParams.append(key, String(v));
          }
        });
      } else {
        if (String(value).trim() !== '') {
          url.searchParams.set(key, String(value));
        }
      }
    });
  }

  return url.pathname + url.search;
}

async function request(method, path, { query, body, headers } = {}) {
  const controller = new AbortController();
  const timeoutMs = getTimeoutMs();
  const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

  const correlationId = createCorrelationId();

  try {
    const url = buildUrl(path, query);

    const res = await fetch(url, {
      method,
      headers: {
        'Content-Type': 'application/json',
        'X-Correlation-ID': correlationId,
        ...headers,
      },
      body: body !== undefined ? JSON.stringify(body) : undefined,
      signal: controller.signal,
    });

    const contentType = res.headers.get('content-type') || '';
    const isJson = contentType.includes('application/json');

    if (!res.ok) {
      let errorPayload = null;
      try {
        errorPayload = isJson ? await res.json() : await res.text();
      } catch {
        errorPayload = null;
      }

      const err = new Error(`HTTP ${res.status} ${res.statusText}`);
      err.status = res.status;
      err.correlationId = correlationId;
      err.payload = errorPayload;
      throw err;
    }

    if (res.status === 204) {
      return null;
    }

    if (isJson) {
      return await res.json();
    }

    return await res.text();
  } finally {
    clearTimeout(timeoutId);
  }
}

export const httpClient = {
  get: (path, options) => request('GET', path, options),
  patch: (path, options) => request('PATCH', path, options),
};
