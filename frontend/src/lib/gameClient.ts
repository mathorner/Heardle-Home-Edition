import { apiBase } from './apiClient';

export type GameSession = {
  gameId: string;
  status: string;
  attempt: number;
  maxAttempts: number;
  createdAt: string;
  updatedAt: string;
};

export type StartGameResult =
  | { type: 'started'; session: GameSession }
  | { type: 'error'; code: string; message: string; status: number };

interface GameClientInit {
  baseUrl?: string;
  signal?: AbortSignal;
}

export async function startGame(init?: GameClientInit): Promise<StartGameResult> {
  const baseUrl = init?.baseUrl ?? apiBase;
  const res = await fetch(`${baseUrl}/game/start`, {
    method: 'POST',
    signal: init?.signal
  });

  if (res.status === 503 || res.status === 409 || res.status === 400) {
    const data = await safeJson(res);
    return {
      type: 'error',
      status: res.status,
      code: typeof data?.code === 'string' ? data.code : 'GameStartFailed',
      message: typeof data?.message === 'string' ? data.message : 'Failed to start game.'
    };
  }

  if (!res.ok) {
    throw new Error(res.statusText);
  }

  const data = await res.json();
  return { type: 'started', session: parseGameSession(data) };
}

export async function getGameSession(id: string, init?: GameClientInit): Promise<GameSession | null> {
  const baseUrl = init?.baseUrl ?? apiBase;
  const res = await fetch(`${baseUrl}/game/${id}`, { signal: init?.signal });

  if (res.status === 404) {
    return null;
  }

  if (!res.ok) {
    const data = await safeJson(res);
    const message = typeof data?.message === 'string' ? data.message : res.statusText;
    const code = typeof data?.code === 'string' ? data.code : 'GameSessionError';
    throw new Error(`${code}: ${message}`);
  }

  const data = await res.json();
  return parseGameSession(data);
}

function parseGameSession(data: any): GameSession {
  if (typeof data?.gameId !== 'string') {
    throw new Error('Invalid game session payload');
  }
  if (typeof data?.status !== 'string') {
    throw new Error('Invalid game session payload');
  }

  const attempt = Number(data?.attempt);
  const maxAttempts = Number(data?.maxAttempts);
  if (!Number.isFinite(attempt) || !Number.isFinite(maxAttempts)) {
    throw new Error('Invalid game session payload');
  }

  return {
    gameId: data.gameId,
    status: data.status,
    attempt,
    maxAttempts,
    createdAt: typeof data?.createdAt === 'string' ? data.createdAt : new Date().toISOString(),
    updatedAt: typeof data?.updatedAt === 'string' ? data.updatedAt : new Date().toISOString()
  };
}

async function safeJson(res: Response): Promise<any> {
  try {
    return await res.json();
  } catch {
    return null;
  }
}
