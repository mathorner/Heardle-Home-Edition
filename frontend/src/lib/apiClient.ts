export const apiBase = import.meta.env.VITE_API_BASE_URL ?? '/api';

// Fetch the server's health status. Accepts an AbortSignal for safe cleanup in React effects.
export async function getHealth(init?: { baseUrl?: string; signal?: AbortSignal }): Promise<string> {
  const baseUrl = init?.baseUrl ?? apiBase;
  const res = await fetch(`${baseUrl}/health`, { signal: init?.signal });
  if (!res.ok) {
    throw new Error(res.statusText);
  }
  const data: { status: string } = await res.json();
  return data.status;
}
