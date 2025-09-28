import { useState } from 'react';
import Home from './components/Home';
import Settings from './components/Settings';
import Game from './components/Game';

export default function App() {
  const [view, setView] = useState<'play' | 'home' | 'settings'>('play');
  return (
    <main>
      <nav style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
        <button onClick={() => setView('play')} aria-pressed={view === 'play'}>
          Play
        </button>
        <button onClick={() => setView('home')} aria-pressed={view === 'home'}>
          Home
        </button>
        <button onClick={() => setView('settings')} aria-pressed={view === 'settings'}>
          Settings
        </button>
      </nav>
      {view === 'play' && <Game onNavigateToSettings={() => setView('settings')} />}
      {view === 'home' && <Home />}
      {view === 'settings' && <Settings />}
    </main>
  );
}
