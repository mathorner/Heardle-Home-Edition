import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import Game from './Game';
import * as gameClient from '../lib/gameClient';

vi.mock('../lib/gameClient', () => ({
  startGame: vi.fn(),
  getGameSession: vi.fn()
}));

const mockStartGame = vi.mocked(gameClient.startGame);
const mockGetGameSession = vi.mocked(gameClient.getGameSession);

describe('Game component', () => {
  beforeEach(() => {
    sessionStorage.clear();
    mockStartGame.mockReset();
    mockGetGameSession.mockReset();
    cleanup();
  });

  it('starts a new game and stores the session id', async () => {
    mockStartGame.mockResolvedValue({
      type: 'started',
      session: {
        gameId: 'game-1',
        status: 'active',
        attempt: 1,
        maxAttempts: 6,
        createdAt: '2025-09-25T12:00:00Z',
        updatedAt: '2025-09-25T12:00:00Z'
      }
    });

    const navigate = vi.fn();
    render(<Game onNavigateToSettings={navigate} />);

    const startButton = screen.getByRole('button', { name: /start game/i });
    fireEvent.click(startButton);

    await waitFor(() => expect(screen.getByText(/Attempt 1 of 6/i)).toBeInTheDocument());
    expect(sessionStorage.getItem('activeGameId')).toBe('game-1');
    expect(navigate).not.toHaveBeenCalled();
  });

  it('resumes existing game from sessionStorage', async () => {
    sessionStorage.setItem('activeGameId', 'existing');
    mockGetGameSession.mockResolvedValue({
      gameId: 'existing',
      status: 'active',
      attempt: 3,
      maxAttempts: 6,
      createdAt: '2025-09-25T12:00:00Z',
      updatedAt: '2025-09-25T12:05:00Z'
    });

    render(<Game onNavigateToSettings={() => {}} />);

    await waitFor(() => expect(screen.getByText(/Attempt 3 of 6/i)).toBeInTheDocument());
  });

  it('shows actionable error when library is not ready', async () => {
    mockStartGame.mockResolvedValue({
      type: 'error',
      status: 503,
      code: 'LibraryNotReady',
      message: 'Run a scan before playing.'
    });

    const navigate = vi.fn();
    render(<Game onNavigateToSettings={navigate} />);

    fireEvent.click(screen.getByRole('button', { name: /start game/i }));

    await waitFor(() => expect(screen.getByRole('alert')).toBeInTheDocument());
    fireEvent.click(screen.getByRole('button', { name: /go to settings/i }));
    expect(navigate).toHaveBeenCalledTimes(1);
  });

  it('clears stored id when session no longer exists', async () => {
    sessionStorage.setItem('activeGameId', 'missing');
    mockGetGameSession.mockResolvedValue(null);

    render(<Game onNavigateToSettings={() => {}} />);

    await waitFor(() => expect(sessionStorage.getItem('activeGameId')).toBeNull());
    expect(screen.getByText(/previous game ended/i)).toBeInTheDocument();
  });
});

