import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import Home from './Home';
import * as api from '../lib/apiClient';

vi.mock('../lib/apiClient', () => ({
  getHealth: vi.fn().mockResolvedValue('ok')
}));

describe('Home', () => {
  it('renders title and fetched health status', async () => {
    render(<Home />);

    expect(
      screen.getByRole('heading', { name: /heardle home edition/i })
    ).toBeInTheDocument();

    expect(await screen.findByText(/api status: ok/i)).toBeInTheDocument();
  });

  it('renders an error state when API call fails', async () => {
    vi.mocked(api.getHealth).mockRejectedValueOnce(new Error('fail'));

    render(<Home />);

    // Renders a paragraph with role="alert" and error text
    const alert = await screen.findByRole('alert');
    expect(alert).toHaveTextContent(/api error/i);
  });
});
