import { render, screen } from '@testing-library/react';
import WorkerDashboardView from './WorkerDashboardView';
import type { WorkerDashboardResponse } from '@/lib/workerDashboardApi';

const mockData: WorkerDashboardResponse = {
  worker: {
    id: '11111111-1111-1111-1111-111111111111',
    name: 'user',
    email: 'user@example.com',
    verified: true,
    blockchainAddress: '0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266',
  },
  summary: {
    pendingReviews: 1,
    reviewedRequests: 2,
    totalRequests: 3,
  },
  latestRequests: [
    {
      requestId: '55555555-5555-5555-5555-555555555551',
      employerId: '22222222-2222-2222-2222-222222222222',
      employerName: 'First Step Solutions',
      requestedInformation: 'PPE requirements',
      checkPurpose: 'Site onboarding',
      createdAt: '2026-04-15T10:00:00Z',
      status: 0,
      expiresAt: null,
    },
    {
      requestId: '55555555-5555-5555-5555-555555555552',
      employerId: '22222222-2222-2222-2222-222222222222',
      employerName: 'First Step Solutions',
      requestedInformation: 'Emergency Contact',
      checkPurpose: 'Emergency planning',
      createdAt: '2026-04-16T10:00:00Z',
      status: 4,
      expiresAt: '2026-06-30T00:00:00Z',
    },
  ],
  blockchainRecords: [],
  blockchainAvailable: true,
};

const mockDataWithBlockchainRecord: WorkerDashboardResponse = {
  ...mockData,
  blockchainRecords: [
    {
      action: 'PermissionApproved',
      actionLabel: 'Access Approved',
      userMessage:
        'You approved First Step Solutions to access your information.',
      employerName: 'First Step Solutions',
      employerAddress: '0x70997970C51812dc3A010C7d01b50e0d17dc79C8',
      workerAddress: '0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266',
      txHash:
        '0x4c8201b58350618d420f27129a6a8fdb27957d23850303c874e07adbebe64cc8',
      date: '2026-05-05T23:56:13Z',
    },
  ],
};

describe('WorkerDashboardView', () => {
  it('renders worker dashboard header and summary cards', () => {
    render(<WorkerDashboardView data={mockData} />);

    expect(screen.getByText('Worker dashboard')).toBeInTheDocument();
    expect(screen.getByText('Welcome back, user')).toBeInTheDocument();

    expect(screen.getAllByText('Pending').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Reviewed').length).toBeGreaterThan(0);
    expect(screen.getByText('Total requests')).toBeInTheDocument();

    expect(screen.queryByText('Active access')).not.toBeInTheDocument();
    expect(screen.queryByText('Blockchain audit')).not.toBeInTheDocument();
  });

  it('renders latest requests without action column', () => {
    render(<WorkerDashboardView data={mockData} />);

    expect(screen.getByText('Latest requests')).toBeInTheDocument();
    expect(screen.getAllByText('First Step Solutions').length).toBeGreaterThan(
      0,
    );
    expect(screen.getByText('PPE requirements')).toBeInTheDocument();
    expect(screen.getByText('Site onboarding')).toBeInTheDocument();
    expect(screen.getByText('Emergency Contact')).toBeInTheDocument();
    expect(screen.getByText('Emergency planning')).toBeInTheDocument();

    expect(screen.getAllByText('Pending').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Reviewed').length).toBeGreaterThan(0);

    expect(screen.queryByText('Action')).not.toBeInTheDocument();
    expect(screen.queryByText('Review')).not.toBeInTheDocument();
  });

  it('renders empty blockchain activity placeholder', () => {
    render(<WorkerDashboardView data={mockData} />);

    expect(screen.getByText('Recent blockchain activity')).toBeInTheDocument();
    expect(
      screen.getByText('No blockchain records available yet.'),
    ).toBeInTheDocument();
  });

  it('renders user-friendly blockchain activity records', () => {
    render(<WorkerDashboardView data={mockDataWithBlockchainRecord} />);

    expect(screen.getByText('Recent blockchain activity')).toBeInTheDocument();
    expect(screen.getByText('Access Approved')).toBeInTheDocument();
    expect(
      screen.getByText(
        'You approved First Step Solutions to access your information.',
      ),
    ).toBeInTheDocument();
    expect(screen.getByText('Proof: 0x4c8201b5...e64cc8')).toBeInTheDocument();
  });

  it('renders blockchain unavailable message', () => {
    render(
      <WorkerDashboardView
        data={{
          ...mockData,
          blockchainAvailable: false,
          blockchainRecords: [],
        }}
      />,
    );

    expect(
      screen.getByText('Blockchain audit is currently unavailable.'),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'Your normal access request flow still works. On-chain audit records will appear here when the blockchain service is running.',
      ),
    ).toBeInTheDocument();
  });
});
