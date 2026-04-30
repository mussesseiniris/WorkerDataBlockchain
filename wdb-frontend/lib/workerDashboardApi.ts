const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5258";

const BASE_URL = `${API_BASE_URL}/api/worker/dashboard`;

export type WorkerBasicInfo = {
  id: string;
  name: string;
  email: string;
  verified: boolean;
};

export type WorkerDashboardRequest = {
  requestId: string;
  employerId: string;
  employerName: string;
  createdAt: string;
  reason: string;
};

export type WorkerDashboardResponse = {
  worker: WorkerBasicInfo;
  latestRequests: WorkerDashboardRequest[];
  blockchainRecords: unknown[];
};

export async function getWorkerDashboard(
  workerId: string
): Promise<WorkerDashboardResponse> {
  const response = await fetch(`${BASE_URL}/${workerId}`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error("Failed to fetch worker dashboard");
  }

  return response.json();
}
