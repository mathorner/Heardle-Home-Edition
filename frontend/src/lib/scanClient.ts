import { apiBase } from './apiClient';

export type StartScanResult =
  | { type: 'started'; status: string; startedAt?: string | null }
  | { type: 'already-running'; status: string }
  | { type: 'error'; code?: string; message: string };

export type ScanStatusSnapshot = {
  status: string;
  total: number;
  indexed: number;
  failed: number;
  startedAt?: string | null;
  finishedAt?: string | null;
};

interface ScanRequestInit {
  baseUrl?: string;
  signal?: AbortSignal;
}

export async function startScan(init?: ScanRequestInit): Promise<StartScanResult> {
  const baseUrl = init?.baseUrl ?? apiBase;
  const res = await fetch(`${baseUrl}/library/scan`, {
    method: 'POST',
    signal: init?.signal
  });

  if (res.status === 409) {
    const data = await safeJson(res);
    return { type: 'already-running', status: typeof data?.status === 'string' ? data.status : 'running' };
  }

  if (res.status === 400) {
    const data = await safeJson(res);
    return {
      type: 'error',
      code: typeof data?.code === 'string' ? data.code : undefined,
      message: typeof data?.message === 'string' ? data.message : 'Failed to start scan'
    };
  }

  if (!res.ok) {
    throw new Error(res.statusText);
  }

  const data = await safeJson(res);
  return {
    type: 'started',
    status: typeof data?.status === 'string' ? data.status : 'running',
    startedAt: data?.startedAt ?? null
  };
}

export async function getScanStatus(init?: ScanRequestInit): Promise<ScanStatusSnapshot> {
  const baseUrl = init?.baseUrl ?? apiBase;
  const res = await fetch(`${baseUrl}/library/status`, { signal: init?.signal });

  if (!res.ok) {
    throw new Error(res.statusText);
  }

  const data = await safeJson(res);
  return {
    status: typeof data?.status === 'string' ? data.status : 'idle',
    total: Number.isFinite(data?.total) ? Number(data!.total) : 0,
    indexed: Number.isFinite(data?.indexed) ? Number(data!.indexed) : 0,
    failed: Number.isFinite(data?.failed) ? Number(data!.failed) : 0,
    startedAt: data?.startedAt ?? null,
    finishedAt: data?.finishedAt ?? null
  };
}

async function safeJson(res: Response): Promise<any> {
  try {
    return await res.json();
  } catch {
    return null;
  }
}
