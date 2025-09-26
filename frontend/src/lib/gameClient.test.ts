import { beforeEach, describe, expect, it, vi } from 'vitest';
import { startGame, getGameSession } from './gameClient';

describe('gameClient.startGame', () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
  });

  it('returns started result when API responds with 201', async () => {
    const payload = {
      gameId: 'abc',
      status: 'active',
      attempt: 1,
      maxAttempts: 6,
      createdAt: '2025-09-25T12:00:00Z',
      updatedAt: '2025-09-25T12:00:00Z'
    };

    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 201,
      json: async () => payload
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof fetch);

    const result = await startGame();

    expect(fetchMock).toHaveBeenCalledWith('/api/game/start', {
      method: 'POST',
      signal: undefined
    });
    expect(result).toEqual({ type: 'started', session: payload });
  });

  it('returns error result for 503 response', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: false,
      status: 503,
      json: async () => ({ code: 'LibraryNotReady', message: 'Run a scan before playing.' })
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof fetch);

    const result = await startGame();

    expect(result).toEqual({
      type: 'error',
      status: 503,
      code: 'LibraryNotReady',
      message: 'Run a scan before playing.'
    });
  });
});

describe('gameClient.getGameSession', () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
  });

  it('returns session data when API responds with 200', async () => {
    const payload = {
      gameId: 'abc',
      status: 'active',
      attempt: 2,
      maxAttempts: 6,
      createdAt: '2025-09-25T12:00:00Z',
      updatedAt: '2025-09-25T12:01:00Z'
    };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => payload
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof fetch);

    const result = await getGameSession('abc');

    expect(fetchMock).toHaveBeenCalledWith('/api/game/abc', { signal: undefined });
    expect(result).toEqual(payload);
  });

  it('returns null when API responds with 404', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: false,
      status: 404,
      json: async () => ({ code: 'GameNotFound' })
    });
    vi.stubGlobal('fetch', fetchMock as unknown as typeof fetch);

    const result = await getGameSession('missing');

    expect(result).toBeNull();
  });
});

