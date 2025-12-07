// ============================================
// Modelos de Autenticación
// ============================================

export interface User {
  id: number;
  name: string;
  email: string;
  role: string;
  createdAt: string;
  lastLoginAt?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: User;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
