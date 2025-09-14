import { useEffect, useState } from 'react';
import { saveLibraryPath, getLibraryPath } from '../lib/settingsClient';

export default function Settings() {
  const [path, setPath] = useState('');
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [currentPath, setCurrentPath] = useState<string | null>(null);

  // Prefill the input by reading the saved path on mount. Non-fatal if missing.
  useEffect(() => {
    let mounted = true;
    getLibraryPath()
      .then((p) => {
        if (!mounted) return;
        if (p) {
          setPath(p);
          setCurrentPath(p);
        }
      })
      .catch(() => {
        /* ignore; absent or error will be handled on save */
      });
    return () => {
      mounted = false;
    };
  }, []);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    setSuccess(null);
    setError(null);
    try {
      const result = await saveLibraryPath(path);
      if (result.saved) {
        setSuccess(`Saved: ${result.path}`);
        setCurrentPath(result.path ?? path);
      } else {
        setError(result.message ?? 'Failed to save');
      }
    } catch (err) {
      setError('Failed to save');
    } finally {
      setSaving(false);
    }
  }

  return (
    <section>
      <h2>Settings</h2>
      {currentPath && <p>Current path: {currentPath}</p>}
      <form onSubmit={onSubmit}>
        <label htmlFor="libraryPath">Library Path</label>
        <input
          id="libraryPath"
          type="text"
          value={path}
          onChange={(e) => setPath(e.target.value)}
          placeholder="/mnt/music or \\nas\\share\\music"
          aria-describedby="libraryPathHelp"
        />
        <p id="libraryPathHelp">Use an absolute path accessible to the server.</p>
        <button type="submit" disabled={saving}>
          {saving ? 'Saving…' : 'Save'}
        </button>
      </form>
      {success && <p aria-live="polite">{success}</p>}
      {error && (
        <p role="alert">{error}</p>
      )}
    </section>
  );
}
