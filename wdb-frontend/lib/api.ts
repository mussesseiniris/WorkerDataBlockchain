// API client: centralized functions for calling the ASP.NET Core backend
const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5258';

  export type UserRole = 'worker' | 'employer';

  export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T | null;
  }

  export interface AuthResult {
    accessToken: string;
    userName: string;
    email: string;
  }

  export async function register(
    role: UserRole,
    email: string,
    userName: string,
    password: string
  ): Promise<ApiResponse<null>> {
    const res = await fetch(`${BASE_URL}/api/${role}s/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, userName, password }),
    });
    return res.json();
  }

  export async function login(
    role: UserRole,
    email: string,
    password: string
  ): Promise<ApiResponse<AuthResult>> {
    const res = await fetch(`${BASE_URL}/api/${role}s/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });
    return res.json();
  }