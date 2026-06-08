export interface WorkerActiveRequest {
  requestId: string;
  employerId: string;
  companyName: string;
  reason: string;
  expiryDate?: string | null;
  createdAt: string;
  items: WorkerRequestReviewItem[];
  customRequest?: WorkerCustomRequest | null;
}

export interface WorkerRequestReviewItem {
  permissionId: string;
  fieldId?: string | null;
  infoId?: string | null;
  label: string;
  category: string;
  type: string;
  value?: string | null;
  status: number;
  hasValue: boolean;
  canApprove: boolean;
  cannotApproveReason?: string | null;
}

export interface WorkerCustomRequest {
  description: string;
  status: string;
}

export interface SubmitWorkerReviewPayload {
  expiryDate?: string | null;
  items: {
    permissionId: string;
    decision: 'approved' | 'rejected';
  }[];
  customRequestDecision?: {
    decision: 'approved' | 'rejected';
    label?: string;
    type?: 'text' | 'file';
    value?: string;
  } | null;
}

export interface ActiveAccessRecord {
  requestId: string;
  companyName: string;
  grantedAt: string;
  reason: string;
  workerInfo: ActiveAccessInfo[];
}

export interface ActiveAccessInfo {
  permissionId: string;
  dataType: string;
  category: string;
  categoryLabel: string;
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5258';

async function readErrorMessage(response: Response, fallback: string) {
  try {
    const data = await response.json();
    return data?.message ?? fallback;
  } catch {
    return fallback;
  }
}

export async function getWorkerActiveRequests(
  token: string,
): Promise<WorkerActiveRequest[]> {
  const response = await fetch(
    `${API_BASE_URL}/api/worker/data-access/active-requests`,
    {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    },
  );

  if (!response.ok) {
    throw new Error(
      await readErrorMessage(response, 'Failed to load active requests.'),
    );
  }

  return response.json();
}

export async function submitWorkerRequestReview(
  token: string,
  requestId: string,
  payload: SubmitWorkerReviewPayload,
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/api/worker/data-access/requests/${requestId}/review`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    },
  );

  if (!response.ok) {
    throw new Error(
      await readErrorMessage(response, 'Failed to submit request review.'),
    );
  }
}

export async function getWorkerActiveAccess(
  token: string,
): Promise<ActiveAccessRecord[]> {
  const response = await fetch(
    `${API_BASE_URL}/api/worker/data-access/active-access`,
    {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    },
  );

  if (!response.ok) {
    throw new Error(
      await readErrorMessage(response, 'Failed to load active access.'),
    );
  }

  return response.json();
}

export async function revokeWorkerActiveAccess(
  token: string,
  requestId: string,
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/api/worker/data-access/active-access/requests/${requestId}/revoke`,
    {
      method: 'PATCH',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    },
  );

  if (!response.ok) {
    throw new Error(
      await readErrorMessage(response, 'Failed to revoke active access.'),
    );
  }
}
