import { describe, it, expect, vi, beforeEach } from 'vitest';
import { getHealth } from './apiClient';

declare global {
  // eslint-disable-next-line no-var
  var fetch: (input: RequestInfo | URL, init?: RequestInit) => Promise<any>;
}

describe('apiClient.getHealth', () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
  });

  it('calls /api/health by default and returns status', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ status: 'ok' })
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof fetch);

    const result = await getHealth();
    expect(result).toBe('ok');
    expect(fetchMock).toHaveBeenCalledWith('/api/health');
  });

  it('uses provided baseUrl when passed', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ status: 'ok' })
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof fetch);

    await getHealth('http://localhost:5158');
    expect(fetchMock).toHaveBeenCalledWith('http://localhost:5158/health');
  });

  it('throws when response is not ok', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: false, statusText: 'Bad Request' });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof fetch);

    await expect(getHealth()).rejects.toThrow('Bad Request');
  });
});

