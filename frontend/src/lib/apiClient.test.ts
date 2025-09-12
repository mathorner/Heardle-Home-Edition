import { vi, describe, it, expect } from 'vitest';
import { getHealth } from './apiClient';

describe('apiClient', () => {
  it('resolves health status', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ status: 'ok' })
    });
    vi.stubGlobal('fetch', fetchMock as any);

    const status = await getHealth('http://api');
    expect(fetchMock).toHaveBeenCalledWith('http://api/health');
    expect(status).toBe('ok');

    vi.unstubAllGlobals();
  });

  it('throws on error response', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: false, statusText: 'fail' });
    vi.stubGlobal('fetch', fetchMock as any);

    await expect(getHealth('http://api')).rejects.toThrow('fail');

    vi.unstubAllGlobals();
  });
});
