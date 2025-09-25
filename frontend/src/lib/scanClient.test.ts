import { beforeEach, describe, expect, it, vi } from 'vitest';
import { startScan, getScanStatus } from './scanClient';

describe('scanClient.startScan', () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
  });

  it('POSTs to /api/library/scan and returns started result on 202', async () => {
    const payload = { status: 'running', startedAt: '2025-09-13T12:00:00Z' };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 202,
      json: async () => payload
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof globalThis.fetch);

    const result = await startScan();

    expect(fetchMock).toHaveBeenCalledWith('/api/library/scan', {
      method: 'POST',
      signal: undefined
    });
    expect(result).toEqual({ type: 'started', status: 'running', startedAt: payload.startedAt });
  });

  it('returns already-running result on 409', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: false,
      status: 409,
      json: async () => ({ status: 'running' })
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof globalThis.fetch);

    const result = await startScan();

    expect(result).toEqual({ type: 'already-running', status: 'running' });
  });

  it('returns error details on 400', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: false,
      status: 400,
      json: async () => ({ code: 'MissingLibraryPath', message: 'Library path is not configured' })
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof globalThis.fetch);

    const result = await startScan();

    expect(result).toEqual({
      type: 'error',
      code: 'MissingLibraryPath',
      message: 'Library path is not configured'
    });
  });
});

describe('scanClient.getScanStatus', () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
  });

  it('fetches current scan status snapshot', async () => {
    const snapshot = {
      status: 'running',
      total: 12,
      indexed: 3,
      failed: 1,
      startedAt: '2025-09-13T12:00:00Z',
      finishedAt: null
    };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => snapshot
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof globalThis.fetch);

    const result = await getScanStatus();

    expect(fetchMock).toHaveBeenCalledWith('/api/library/status', { signal: undefined });
    expect(result).toEqual(snapshot);
  });

  it('throws when response is not ok', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: false, status: 500, statusText: 'Server Error' });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof globalThis.fetch);

    await expect(getScanStatus()).rejects.toThrow('Server Error');
  });
});
