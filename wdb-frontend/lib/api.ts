// API client: centralized functions for calling the ASP.NET Core backend

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5258";
const BASE_URL = `${API_BASE_URL}/api/worker/dataaccess`

export type PermissionRequests = {
    id: string;
    workerid: string;
    infoid: string;
    status: string;
    requestid: string;
    expirydate: string;
    lastupdatedat: string;
}

export type WorkerPermissionResponse = {
    permissions: PermissionRequests[];
}


export async function getPermissions(
  workerId: string
): Promise<WorkerPermissionResponse> {
  const response = await fetch(`${BASE_URL}/${workerId}`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error("Failed to fetch worker permission");
  }

  return response.json();
}