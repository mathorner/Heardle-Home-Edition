import { useState } from 'react';
import Home from './components/Home';
import Settings from './components/Settings';

export default function App() {
  const [view, setView] = useState<'home' | 'settings'>('home');
  return (
    <main>
      <nav style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
        <button onClick={() => setView('home')} aria-pressed={view === 'home'}>
          Home
        </button>
        <button onClick={() => setView('settings')} aria-pressed={view === 'settings'}>
          Settings
        </button>
      </nav>
      {view === 'home' ? <Home /> : <Settings />}
    </main>
  );
}
