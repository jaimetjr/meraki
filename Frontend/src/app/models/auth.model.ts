export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  role: string;
  userId: string;
  expiresAt: string;
}

export interface AuthUser {
  token: string;
  email: string;
  role: string;
  userId: string;
  expiresAt: string;
}

