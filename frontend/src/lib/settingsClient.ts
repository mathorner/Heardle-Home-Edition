export const apiBase = import.meta.env.VITE_API_BASE_URL ?? '/api';

export type SaveLibraryPathResult = {
  saved: boolean;
  path?: string;
  code?: string;
  message?: string;
};

// Persist the library path via the settings endpoint. Returns a structured result used by the UI.
export async function saveLibraryPath(path: string, baseUrl: string = apiBase): Promise<SaveLibraryPathResult> {
  const res = await fetch(`${baseUrl}/settings/library-path`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ path }),
  });
  const json = await res.json().catch(() => ({}));
  if (!res.ok) {
    return { saved: false, code: json.code ?? 'Unknown', message: json.message ?? 'Failed to save' };
  }
  return json as SaveLibraryPathResult;
}

// Retrieve the saved library path (or null when not set yet).
export async function getLibraryPath(baseUrl: string = apiBase): Promise<string | null> {
  const res = await fetch(`${baseUrl}/settings/library-path`);
  if (res.status === 404) return null;
  if (!res.ok) throw new Error(res.statusText);
  const json = (await res.json()) as { path?: string };
  return json.path ?? null;
}
