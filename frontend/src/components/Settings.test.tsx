import { render, screen } from '@testing-library/react';
import { vi } from 'vitest';
import Settings from './Settings';
import * as client from '../lib/settingsClient';

vi.mock('../lib/settingsClient', () => ({
  saveLibraryPath: vi.fn().mockResolvedValue({ saved: true, path: '/tmp/music' }),
  getLibraryPath: vi.fn().mockResolvedValue('/tmp/music')
}));

describe('Settings', () => {
  it('saves a valid path and shows confirmation', async () => {
    render(<Settings />);

    const input = await screen.findByLabelText(/library path/i);
    // Prefilled from getLibraryPath mock
    expect((input as HTMLInputElement).value).toBe('/tmp/music');

    const btn = screen.getByRole('button', { name: /save/i });
    btn.click();

    expect(await screen.findByText(/saved: \/tmp\/music/i)).toBeInTheDocument();
  });

  it('shows error when API returns failure', async () => {
    vi.mocked(client.saveLibraryPath).mockResolvedValueOnce({ saved: false, code: 'InvalidPath', message: 'Path is required.' });

    render(<Settings />);

    const btn = screen.getByRole('button', { name: /save/i });
    btn.click();

    const alert = await screen.findByRole('alert');
    expect(alert).toHaveTextContent(/path is required/i);
  });
});
