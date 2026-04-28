// API client: centralized functions for calling the ASP.NET Core backend

import { error } from 'console';

const BASE_URL = process.env.NEXT_PUBLIC_API_URL;

export async function FetchApi(endpoint: string, options?: RequestInit) {
  const result = await fetch(`${BASE_URL}${endpoint}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!result.ok) {
    throw new Error(`Http error: ${result.status}`);
  }
  return result.json();

}