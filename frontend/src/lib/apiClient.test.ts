import { describe, it, expect, vi, beforeEach } from 'vitest';
import { getHealth } from './apiClient';

describe('apiClient.getHealth', () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
  });

  it('calls /api/health by default and returns status', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ status: 'ok' })
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof globalThis.fetch);

    const result = await getHealth();
    expect(result).toBe('ok');
    expect(fetchMock).toHaveBeenCalledWith('/api/health', { signal: undefined });
  });

  it('uses provided baseUrl when passed', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ status: 'ok' })
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof globalThis.fetch);

    await getHealth({ baseUrl: 'http://localhost:5158' });
    expect(fetchMock).toHaveBeenCalledWith('http://localhost:5158/health', { signal: undefined });
  });

  it('throws when response is not ok', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: false, statusText: 'Bad Request' });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof globalThis.fetch);

    await expect(getHealth()).rejects.toThrow('Bad Request');
  });
});
