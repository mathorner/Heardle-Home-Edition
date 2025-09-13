import { useEffect, useState } from 'react';
import { getHealth } from '../lib/apiClient';

export default function Home() {
  const [status, setStatus] = useState<string>('');
  const [error, setError] = useState(false);

  useEffect(() => {
    const ac = new AbortController();
    getHealth({ signal: ac.signal })
      .then(setStatus)
      .catch((err: unknown) => {
        // Ignore aborts (React 18 StrictMode runs effects twice in dev)
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setError(true);
      });
    return () => ac.abort();
  }, []);

  return (
    <main>
      <h1>Heardle Home Edition</h1>
      {error ? (
        <p role="alert">API error</p>
      ) : (
        <p aria-live="polite">API status: {status}</p>
      )}
    </main>
  );
}
