export type EmployerCompanyInfo = {
  name: string;
  email: string;
  verified: boolean;
};

export type EmployerDashboardSummary = {
  pendingRequests: number;
  availableRequests: number;
  partialRequests: number;
};

export type EmployerRecentRequest = {
  requestId: string;
  workerName: string;
  requestedFields: string[];
  reason: string;
  status: 'Pending' | 'Available' | 'Partial' | 'Unavailable';
  lastUpdatedAt: string;
};

export type EmployerDashboardData = {
  company: EmployerCompanyInfo;
  summary: EmployerDashboardSummary;
  recentRequests: EmployerRecentRequest[];
};

type ApiResponse<T> = {
  success: boolean;
  data: T;
  message?: string;
  error?: {
    code?: string;
    message?: string;
  };
};

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5258';

export async function getEmployerDashboardMe(
  accessToken: string
): Promise<EmployerDashboardData> {
  const response = await fetch(`${API_BASE_URL}/api/employers/me/dashboard`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
  });

  const result = (await response.json()) as ApiResponse<EmployerDashboardData>;

  if (!response.ok || !result.success) {
    throw new Error(
      result.error?.message ||
        result.message ||
        'Failed to load employer dashboard.'
    );
  }

  return result.data;
}
