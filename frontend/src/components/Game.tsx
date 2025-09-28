import { useEffect, useState } from 'react';
import type { GameSession } from '../lib/gameClient';
import { getGameSession, startGame } from '../lib/gameClient';

const STORAGE_KEY = 'activeGameId';

type GameProps = {
  onNavigateToSettings: () => void;
};

type ErrorState = {
  code?: string;
  message: string;
};

export default function Game({ onNavigateToSettings }: GameProps) {
  const [session, setSession] = useState<GameSession | null>(null);
  const [loading, setLoading] = useState(false);
  const [rehydrating, setRehydrating] = useState(false);
  const [error, setError] = useState<ErrorState | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  useEffect(() => {
    const storedId = window.sessionStorage.getItem(STORAGE_KEY);
    if (!storedId) return;

    setRehydrating(true);
    setInfo('Restoring your active game…');

    getGameSession(storedId)
      .then((result) => {
        if (!result) {
          window.sessionStorage.removeItem(STORAGE_KEY);
          setInfo('Previous game ended. Start a new round when you are ready.');
          return;
        }

        setSession(result);
        setInfo('Resumed your active game.');
      })
      .catch(() => {
        setError({ message: 'Failed to load existing game.' });
      })
      .finally(() => {
        setRehydrating(false);
      });
  }, []);

  async function handleStartClick() {
    setLoading(true);
    setError(null);
    setInfo(null);

    try {
      const result = await startGame();
      if (result.type === 'started') {
        setSession(result.session);
        window.sessionStorage.setItem(STORAGE_KEY, result.session.gameId);
        setInfo('Round ready. Audio snippets unlock in the next iteration.');
      } else {
        window.sessionStorage.removeItem(STORAGE_KEY);
        setSession(null);
        setError({ code: result.code, message: result.message });
      }
    } catch (err) {
      setError({ message: 'Failed to start game.' });
    } finally {
      setLoading(false);
    }
  }

  const statusText = (() => {
    if (session) {
      return `Attempt ${session.attempt} of ${session.maxAttempts}`;
    }
    if (rehydrating) {
      return 'Restoring previous game…';
    }
    return 'No active game yet. Start when you are ready!';
  })();

  const showSettingsCta = error && (error.code === 'LibraryNotReady' || error.code === 'NoTracksIndexed');

  return (
    <section
      style={{
        display: 'grid',
        gap: '1.25rem',
        padding: '1rem 0'
      }}
    >
      <header style={{ display: 'grid', gap: '0.5rem' }}>
        <h2>Play</h2>
        <p style={{ margin: 0 }}>Start a game to hear song snippets and guess the title and artist.</p>
      </header>

      {error && (
        <div role="alert" style={{ display: 'grid', gap: '0.5rem' }}>
          <p style={{ margin: 0 }}>{error.message}</p>
          {showSettingsCta && (
            <button type="button" onClick={onNavigateToSettings} style={{ width: 'fit-content' }}>
              Go to Settings to scan library
            </button>
          )}
        </div>
      )}

      {info && (
        <p role="status" style={{ margin: 0 }}>
          {info}
        </p>
      )}

      <button
        type="button"
        onClick={handleStartClick}
        disabled={loading || rehydrating}
        style={{
          padding: '0.75rem 1rem',
          fontSize: '1rem',
          borderRadius: '0.5rem'
        }}
      >
        {loading ? 'Starting…' : 'Start Game'}
      </button>

      <p aria-live="polite" style={{ margin: 0 }}>
        {statusText}
      </p>

      {session && (
        <div style={{ display: 'grid', gap: '0.5rem' }}>
          <p style={{ margin: 0 }}>
            Keep this tab open. Audio snippet playback is coming in the next iteration.
          </p>
          <p style={{ margin: 0, fontSize: '0.875rem', color: '#555' }}>
            Game created at {new Date(session.createdAt).toLocaleString()}
          </p>
        </div>
      )}
    </section>
  );
}

