import { useEffect, useState } from 'react';
import { getHealth } from '../lib/apiClient';

export default function Home() {
  const [status, setStatus] = useState<string>('');
  const [error, setError] = useState(false);

  useEffect(() => {
    getHealth()
      .then(setStatus)
      .catch(() => setError(true));
  }, []);

  return (
    <main>
      <h1>Heardle Home Edition</h1>
      {error ? (
        <p role="alert">API error</p>
      ) : (
        <p>API status: {status}</p>
      )}
    </main>
  );
}
