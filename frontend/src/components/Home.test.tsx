import { render, screen, waitFor } from '@testing-library/react';
import { vi, describe, it, expect } from 'vitest';
import Home from './Home';

describe('Home', () => {
  it('renders title and health status', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ status: 'ok' })
    });
    vi.stubGlobal('fetch', fetchMock as any);

    render(<Home />);

    expect(screen.getByText(/Heardle Home Edition/i)).toBeInTheDocument();
    await waitFor(() => expect(screen.getByText(/API status: ok/i)).toBeInTheDocument());

    vi.unstubAllGlobals();
  });
});
