import { mapHttpStatusToFeedback } from '../utils/httpStatus';
import { ApiResponse } from './backendDtos';

const TOKEN_KEY = 'hcshop_access_token';
const EXPIRES_KEY = 'hcshop_token_expires_at';

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/+$/, '');

type UnauthorizedListener = () => void;
let unauthorizedListener: UnauthorizedListener | null = null;

export function setUnauthorizedListener(listener: UnauthorizedListener | null): void {
  unauthorizedListener = listener;
}

export function getAccessToken(): string | null {
  try {
    const token = localStorage.getItem(TOKEN_KEY);
    if (!token) return null;
    const expiresAt = localStorage.getItem(EXPIRES_KEY);
    if (expiresAt) {
      const expMs = new Date(expiresAt).getTime();
      if (Number.isFinite(expMs) && expMs <= Date.now()) {
        clearAccessToken();
        return null;
      }
    }
    return token;
  } catch {
    return null;
  }
}

export function setAccessToken(token: string, expiresAt?: string): void {
  try {
    localStorage.setItem(TOKEN_KEY, token);
    if (expiresAt) {
      localStorage.setItem(EXPIRES_KEY, expiresAt);
    }
  } catch {
    // Ignore storage quota errors
  }
}

export function clearAccessToken(): void {
  try {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EXPIRES_KEY);
  } catch {
    // Ignore storage errors
  }
}

export function buildApiUrl(path: string): string {
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${API_BASE_URL}${normalizedPath}`;
}

export function resolveAssetUrl(url?: string | null): string {
  if (!url) return '';
  const trimmed = url.trim();
  if (!trimmed) return '';
  if (
    trimmed.startsWith('http://') ||
    trimmed.startsWith('https://') ||
    trimmed.startsWith('data:') ||
    trimmed.startsWith('blob:')
  ) {
    return trimmed;
  }
  const normalized = trimmed.startsWith('/') ? trimmed : `/${trimmed}`;
  return `${API_BASE_URL}${normalized}`;
}

export interface ApiResult<T> {
  ok: boolean;
  status: number;
  message: string;
  data?: T;
  errors?: unknown;
}

export interface ApiRequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  body?: unknown;
  query?: Record<string, string | number | boolean | null | undefined>;
  skipAuthRedirect?: boolean;
}

function buildQueryString(
  query?: Record<string, string | number | boolean | null | undefined>
): string {
  if (!query) return '';
  const params = new URLSearchParams();
  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      params.append(key, String(value));
    }
  });
  const str = params.toString();
  return str ? `?${str}` : '';
}

function extractValidationErrorMessage(errors: unknown): string | undefined {
  if (!errors || typeof errors !== 'object') return undefined;
  const values = Object.values(errors as Record<string, unknown>);
  for (const val of values) {
    if (Array.isArray(val) && val.length > 0 && typeof val[0] === 'string') {
      return val[0];
    }
    if (typeof val === 'string' && val.trim()) {
      return val;
    }
  }
  return undefined;
}

export async function apiRequest<T>(
  path: string,
  options: ApiRequestOptions = {}
): Promise<ApiResult<T>> {
  const method = options.method || 'GET';
  const url = buildApiUrl(`${path}${buildQueryString(options.query)}`);
  const headers: Record<string, string> = {
    Accept: 'application/json',
  };

  const token = getAccessToken();
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  let bodyPayload: BodyInit | undefined;
  if (options.body !== undefined) {
    if (options.body instanceof FormData) {
      bodyPayload = options.body;
    } else {
      headers['Content-Type'] = 'application/json';
      bodyPayload = JSON.stringify(options.body);
    }
  }

  try {
    const response = await fetch(url, {
      method,
      headers,
      body: bodyPayload,
    });

    const contentType = response.headers.get('content-type') || '';
    const json = contentType.includes('application/json')
      ? ((await response.json().catch(() => null)) as ApiResponse<T> | null)
      : null;

    if (!response.ok || (json && json.success === false)) {
      if (response.status === 401 && !options.skipAuthRedirect) {
        clearAccessToken();
        unauthorizedListener?.();
      }
      const detailError = extractValidationErrorMessage(json?.errors);
      const serverMsg = detailError || json?.message;
      const mapped = mapHttpStatusToFeedback(response.status, serverMsg);
      return {
        ok: false,
        status: response.status,
        message: mapped.message,
        errors: json?.errors,
      };
    }

    return {
      ok: true,
      status: response.status,
      message: json?.message || 'Thành công.',
      data: json ? json.data : undefined,
    };
  } catch {
    return {
      ok: false,
      status: 0,
      message:
        'Không thể kết nối tới hệ thống. Vui lòng kiểm tra kết nối mạng và thử lại.',
    };
  }
}

export async function apiUploadImage(
  file: File,
  folder: 'products' | 'categories' | 'avatars' | 'reviews'
): Promise<ApiResult<{ url: string; fileName: string }>> {
  const formData = new FormData();
  formData.append('file', file);
  return apiRequest<{ url: string; fileName: string }>('/api/upload/hinh-anh', {
    method: 'POST',
    query: { folder },
    body: formData,
  });
}

export async function apiFetchHtmlReport(
  query: Record<string, string | number | boolean | null | undefined>
): Promise<ApiResult<string>> {
  const url = buildApiUrl(`/api/bao-cao/in-bao-cao${buildQueryString(query)}`);
  const headers: Record<string, string> = {
    Accept: 'text/html, application/json',
  };
  const token = getAccessToken();
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  try {
    const response = await fetch(url, { method: 'GET', headers });
    const contentType = response.headers.get('content-type') || '';

    if (!response.ok) {
      if (response.status === 401) {
        clearAccessToken();
        unauthorizedListener?.();
      }
      let serverMsg: string | undefined;
      if (contentType.includes('application/json')) {
        const json = (await response.json().catch(() => null)) as ApiResponse<unknown> | null;
        serverMsg = json?.message;
      }
      const mapped = mapHttpStatusToFeedback(response.status, serverMsg);
      return { ok: false, status: response.status, message: mapped.message };
    }

    const html = await response.text();
    return { ok: true, status: response.status, message: 'Thành công.', data: html };
  } catch {
    return {
      ok: false,
      status: 0,
      message: 'Không thể chuẩn bị bản in báo cáo lúc này. Vui lòng thử lại sau.',
    };
  }
}
