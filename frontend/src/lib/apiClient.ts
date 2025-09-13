export const apiBase = import.meta.env.VITE_API_BASE_URL ?? '/api';

export async function getHealth(baseUrl: string = apiBase): Promise<string> {
  const res = await fetch(`${baseUrl}/health`);
  if (!res.ok) {
    throw new Error(res.statusText);
  }
  const data: { status: string } = await res.json();
  return data.status;
}
