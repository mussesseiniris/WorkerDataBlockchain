const BASE_URL = `${process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5258'
  }/api/employer-request`;

export interface EmployerRequestCatalogWorker {
  id: string;
  name: string;
  email: string;
}

export interface EmployerRequestCatalogPresetField {
  fieldId: string;
  label: string;
  allowedType: 'text' | 'file';
}

export interface EmployerRequestCatalogCustomItem {
  workerInfoId: string;
  label: string;
  type: 'text' | 'file';
}

export interface EmployerRequestCatalogCategory {
  id: string;
  name: string;
  presetFields: EmployerRequestCatalogPresetField[];
  customItems: EmployerRequestCatalogCustomItem[];
}

export interface EmployerRequestCatalog {
  worker: EmployerRequestCatalogWorker;
  categories: EmployerRequestCatalogCategory[];
}

export interface CreateEmployerRequestPayload {
  workerEmail: string;
  reason: string;
  presetFieldIds: string[];
  customWorkerInfoIds: string[];
  customRequest?: string;
}

export interface CreateEmployerRequestResult {
  requestId: string;
}

export async function fetchRequestCatalog(
  token: string,
  email: string,
): Promise<EmployerRequestCatalog> {
  const res = await fetch(
    `${BASE_URL}/catalog?email=${encodeURIComponent(email)}`,
    {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    },
  );

  if (!res.ok) {
    throw new Error(`Failed to load catalog (${res.status})`);
  }

  return res.json();
}

export async function createEmployerRequest(
  token: string,
  payload: CreateEmployerRequestPayload,
): Promise<CreateEmployerRequestResult> {
  const res = await fetch(BASE_URL, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    throw new Error(`Failed to create request (${res.status})`);
  }

  return res.json();
}
