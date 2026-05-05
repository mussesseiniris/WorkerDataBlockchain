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
    availableRequests: 1,
    partialRequests: 1,
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
      requestedFields: ['Training Status'],
      reason: 'Project qualification',
      status: 'Available',
      lastUpdatedAt: '2026-04-28T10:20:00.000Z',
    },
    {
      requestId: 'request-3',
      workerName: 'Alyanna',
      requestedFields: ['Emergency Contact'],
      reason: 'Emergency preparedness',
      status: 'Partial',
      lastUpdatedAt: '2026-04-27T10:20:00.000Z',
    },
    {
      requestId: 'request-4',
      workerName: 'Jason',
      requestedFields: ['PPE Requirement'],
      reason: 'Site safety check',
      status: 'Unavailable',
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

  it('renders request summary cards', () => {
    render(<EmployerDashboardView data={mockDashboardData} />);

    expect(screen.getByText('Pending Requests')).toBeInTheDocument();
    expect(screen.getByText('Available Requests')).toBeInTheDocument();
    expect(screen.getByText('Partial Requests')).toBeInTheDocument();

    expect(screen.getByText('2')).toBeInTheDocument();

    // There are two summary values with "1", so use getAllByText.
    expect(screen.getAllByText('1')).toHaveLength(2);
  });

  it('renders recent access request details', () => {
    render(<EmployerDashboardView data={mockDashboardData} />);

    expect(screen.getByText('Recent Access Requests')).toBeInTheDocument();

    expect(screen.getByText('Will')).toBeInTheDocument();
    expect(screen.getByText('Phone, Address')).toBeInTheDocument();
    expect(screen.getByText('Safety compliance check')).toBeInTheDocument();

    expect(screen.getByText('Luca')).toBeInTheDocument();
    expect(screen.getByText('Training Status')).toBeInTheDocument();
    expect(screen.getByText('Project qualification')).toBeInTheDocument();

    expect(screen.getByText('Alyanna')).toBeInTheDocument();
    expect(screen.getByText('Emergency Contact')).toBeInTheDocument();
    expect(screen.getByText('Emergency preparedness')).toBeInTheDocument();

    expect(screen.getByText('Jason')).toBeInTheDocument();
    expect(screen.getByText('PPE Requirement')).toBeInTheDocument();
    expect(screen.getByText('Site safety check')).toBeInTheDocument();
  });

  it('renders all supported request statuses', () => {
    render(<EmployerDashboardView data={mockDashboardData} />);

    expect(screen.getAllByText('Pending').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Available').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Partial').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Unavailable').length).toBeGreaterThan(0);
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
        availableRequests: 0,
        partialRequests: 0,
      },
      recentRequests: [],
    };

    render(<EmployerDashboardView data={emptyData} />);

    expect(screen.getByText('No recent access requests.')).toBeInTheDocument();
  });
});
