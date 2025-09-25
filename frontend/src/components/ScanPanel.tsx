import { useEffect, useRef, useState } from 'react';
import { getScanStatus, startScan, type ScanStatusSnapshot } from '../lib/scanClient';

const initialSnapshot: ScanStatusSnapshot = {
  status: 'idle',
  total: 0,
  indexed: 0,
  failed: 0,
  startedAt: null,
  finishedAt: null
};

const POLL_INTERVAL_MS = 750;

export default function ScanPanel() {
  const [snapshot, setSnapshot] = useState<ScanStatusSnapshot>(initialSnapshot);
  const [starting, setStarting] = useState(false);
  const [info, setInfo] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const pollRef = useRef<number | null>(null);
  const mountedRef = useRef(true);

  useEffect(() => {
    mountedRef.current = true;
    const ac = new AbortController();
    refreshStatus(ac.signal);

    return () => {
      mountedRef.current = false;
      ac.abort();
      stopPolling();
    };
  }, []);

  async function refreshStatus(signal?: AbortSignal) {
    try {
      const next = await getScanStatus({ signal });
      if (!mountedRef.current) return;
      setSnapshot(next);
      setError(null);

      if (next.status === 'running') {
        startPolling();
      } else {
        stopPolling();
        if (next.status === 'completed') {
          setInfo('Scan completed.');
        }
      }
    } catch (err) {
      if (!mountedRef.current) return;
      if (isAbortError(err)) return;
      setError('Failed to load scan status');
    }
  }

  function startPolling() {
    if (pollRef.current !== null) return;
    pollRef.current = window.setInterval(() => {
      const controller = new AbortController();
      refreshStatus(controller.signal).finally(() => controller.abort());
    }, POLL_INTERVAL_MS);
  }

  function stopPolling() {
    if (pollRef.current !== null) {
      window.clearInterval(pollRef.current);
      pollRef.current = null;
    }
  }

  async function onStartClick() {
    setStarting(true);
    setInfo(null);
    setError(null);
    try {
      const result = await startScan();
      if (!mountedRef.current) return;

      if (result.type === 'already-running') {
        setInfo('Scan already running.');
        setSnapshot((prev) => ({ ...prev, status: result.status }));
        startPolling();
        await refreshStatus();
      } else if (result.type === 'error') {
        setError(result.message);
      } else {
        setSnapshot((prev) => ({
          ...prev,
          status: result.status,
          startedAt: result.startedAt ?? prev.startedAt ?? null,
          finishedAt: null
        }));
        startPolling();
        await refreshStatus();
      }
    } catch (err) {
      if (!isAbortError(err)) {
        setError('Failed to start scan');
      }
    } finally {
      if (mountedRef.current) {
        setStarting(false);
      }
    }
  }

  const running = snapshot.status === 'running';
  const buttonDisabled = starting || running;

  let progressText = 'No scan in progress.';
  if (snapshot.status === 'running') {
    progressText = `Running – Indexed ${snapshot.indexed} of ${snapshot.total} (${snapshot.failed} failed)`;
  } else if (snapshot.status === 'completed') {
    progressText = `Scan completed – Indexed ${snapshot.indexed} of ${snapshot.total} (${snapshot.failed} failed)`;
  }

  return (
    <section aria-labelledby="scan-panel-heading">
      <h2 id="scan-panel-heading">Library Scan</h2>
      <p>Start a scan of your music library and watch progress.</p>
      <button type="button" onClick={onStartClick} disabled={buttonDisabled}>
        {running ? 'Scanning…' : 'Scan Now'}
      </button>
      {info && <p role="status">{info}</p>}
      {error && <p role="alert">{error}</p>}
      <p aria-live="polite">{progressText}</p>
    </section>
  );
}

function isAbortError(err: unknown): boolean {
  return err instanceof DOMException && err.name === 'AbortError';
}
