import { render, screen } from '@testing-library/react';
import EmployerDashboardView from './EmployerDashboardView';
import type { EmployerDashboardData } from '@/lib/employerDashboardApi';

const mockDashboardData: EmployerDashboardData = {
  company: {
    name: 'Acme Construction Co.',
    email: 'admin@acme.com',
    verified: false,
  },
  summary: {
    pendingRequests: 2,
    reviewedRequests: 3,
    totalRequests: 5,
  },
  recentRequests: [
    {
      requestId: 'request-1',
      workerName: 'Will',
      requestedFields: ['Phone', 'Address'],
      reason: 'Safety compliance check',
      status: 'Pending',
      lastUpdatedAt: '2026-04-29T23:45:42.873881Z',
    },
    {
      requestId: 'request-2',
      workerName: 'Luca',
      requestedFields: [
        'Training Status',
        'PPE Requirement',
        'Emergency Contact',
        'Medical Notes',
        'Address',
      ],
      reason: 'Project qualification',
      status: 'Approved',
      lastUpdatedAt: '2026-04-28T10:20:00.000Z',
    },
    {
      requestId: 'request-3',
      workerName: 'Alyanna',
      requestedFields: ['Emergency Contact'],
      reason: 'Emergency preparedness',
      status: 'PartiallyApproved',
      lastUpdatedAt: '2026-04-27T10:20:00.000Z',
    },
    {
      requestId: 'request-4',
      workerName: 'Jason',
      requestedFields: ['PPE Requirement'],
      reason: 'Site safety check',
      status: 'Revoked',
      lastUpdatedAt: '2026-04-26T10:20:00.000Z',
    },
  ],
};

describe('EmployerDashboardView', () => {
  it('renders the employer dashboard heading', () => {
    render(<EmployerDashboardView data={mockDashboardData} />);

    expect(
      screen.getByRole('heading', { name: /employer dashboard/i })
    ).toBeInTheDocument();
  });

  it('renders company information', () => {
    render(<EmployerDashboardView data={mockDashboardData} />);

    expect(screen.getByText('Company Information')).toBeInTheDocument();
    expect(screen.getByText('Acme Construction Co.')).toBeInTheDocument();
    expect(screen.getByText('admin@acme.com')).toBeInTheDocument();
    expect(screen.getByText('Not verified')).toBeInTheDocument();
  });

  it('renders simplified request summary cards', () => {
    render(<EmployerDashboardView data={mockDashboardData} />);

    expect(screen.getAllByText('Pending').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Reviewed').length).toBeGreaterThan(0);
    expect(screen.getByText('Total Requests')).toBeInTheDocument();

    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
    expect(screen.getByText('5')).toBeInTheDocument();

    expect(screen.queryByText('Partially Approved')).not.toBeInTheDocument();
    expect(screen.queryByText('Approved')).not.toBeInTheDocument();
    expect(screen.queryByText('Active Access')).not.toBeInTheDocument();
  });

  it('renders recent access request details with field preview', () => {
    render(<EmployerDashboardView data={mockDashboardData} />);

    expect(screen.getByText('Recent Access Requests')).toBeInTheDocument();
    expect(screen.getByText('View all requests')).toBeInTheDocument();

    expect(screen.getByText('Will')).toBeInTheDocument();
    expect(screen.getByText('Phone, Address')).toBeInTheDocument();
    expect(screen.getByText('Safety compliance check')).toBeInTheDocument();

    expect(screen.getByText('Luca')).toBeInTheDocument();
    expect(
      screen.getByText('Training Status, PPE Requirement, Emergency Contact +2 more')
    ).toBeInTheDocument();
    expect(screen.getByText('Project qualification')).toBeInTheDocument();
  });

  it('shows only Pending, Reviewed, and Revoked status labels on dashboard', () => {
    render(<EmployerDashboardView data={mockDashboardData} />);

    expect(screen.getAllByText('Pending').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Reviewed').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Revoked').length).toBeGreaterThan(0);

    expect(screen.queryByText('Partially Approved')).not.toBeInTheDocument();
    expect(screen.queryByText('Rejected')).not.toBeInTheDocument();
    expect(screen.queryByText('Approved')).not.toBeInTheDocument();
  });

  it('shows an empty state when there are no recent requests', () => {
    const emptyData: EmployerDashboardData = {
      company: {
        name: 'Acme Construction Co.',
        email: 'admin@acme.com',
        verified: true,
      },
      summary: {
        pendingRequests: 0,
        reviewedRequests: 0,
        totalRequests: 0,
      },
      recentRequests: [],
    };

    render(<EmployerDashboardView data={emptyData} />);

    expect(screen.getByText('No recent access requests.')).toBeInTheDocument();
  });
});
