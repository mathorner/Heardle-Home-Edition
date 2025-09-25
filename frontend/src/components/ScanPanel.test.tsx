import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ScanPanel from './ScanPanel';
import * as scanClient from '../lib/scanClient';

vi.mock('../lib/scanClient', () => ({
  startScan: vi.fn(),
  getScanStatus: vi.fn()
}));

const mockStartScan = vi.mocked(scanClient.startScan);
const mockGetScanStatus = vi.mocked(scanClient.getScanStatus);

describe('ScanPanel', () => {
  beforeEach(() => {
    mockStartScan.mockReset();
    mockGetScanStatus.mockReset();
  });

  it('disables the button while running and shows progress updates', async () => {
    mockGetScanStatus
      .mockResolvedValueOnce({
        status: 'idle',
        total: 0,
        indexed: 0,
        failed: 0,
        startedAt: null,
        finishedAt: null
      })
      .mockResolvedValueOnce({
        status: 'running',
        total: 10,
        indexed: 3,
        failed: 1,
        startedAt: '2025-09-13T12:00:00Z',
        finishedAt: null
      });
    mockStartScan.mockResolvedValue({ type: 'started', status: 'running', startedAt: '2025-09-13T12:00:00Z' });

    render(<ScanPanel />);

    const button = await screen.findByRole('button', { name: /scan now/i });
    expect(button).not.toBeDisabled();

    fireEvent.click(button);

    await waitFor(() => expect(button).toBeDisabled());
    expect(await screen.findByText(/Indexed 3 of 10/i)).toBeInTheDocument();
    expect(mockGetScanStatus.mock.calls.length).toBeGreaterThanOrEqual(2);
  });

  it('shows completion summary when scan has finished', async () => {
    mockGetScanStatus.mockResolvedValue({
      status: 'completed',
      total: 12,
      indexed: 11,
      failed: 1,
      startedAt: '2025-09-13T12:00:00Z',
      finishedAt: '2025-09-13T12:03:00Z'
    });

    render(<ScanPanel />);

    expect(await screen.findByText(/scan completed – indexed 11 of 12/i)).toBeInTheDocument();
    expect(screen.getByText(/Indexed 11 of 12/i)).toBeInTheDocument();
    const button = screen.getByRole('button', { name: /scan now/i });
    expect(button).not.toBeDisabled();
  });
});
