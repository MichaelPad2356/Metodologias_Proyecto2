import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, of } from 'rxjs';
import { Router } from '@angular/router';
import {
  User,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  ChangePasswordRequest
} from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = '/api/auth';
  private readonly TOKEN_KEY = 'openup_token';
  private readonly USER_KEY = 'openup_user';

  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.loadStoredUser();
  }

  /**
   * Cargar usuario almacenado en localStorage
   */
  private loadStoredUser(): void {
    const token = localStorage.getItem(this.TOKEN_KEY);
    const userStr = localStorage.getItem(this.USER_KEY);

    if (token && userStr) {
      try {
        const user = JSON.parse(userStr) as User;
        this.currentUserSubject.next(user);
        this.isAuthenticatedSubject.next(true);
        // Verificar que el token siga siendo válido
        this.verifyToken();
      } catch {
        this.clearAuth();
      }
    }
  }

  /**
   * Verificar token con el backend
   */
  private verifyToken(): void {
    this.http.get<AuthResponse>(`${this.apiUrl}/me`).pipe(
      catchError(() => {
        this.clearAuth();
        return of(null);
      })
    ).subscribe(response => {
      if (response?.success && response.user) {
        this.currentUserSubject.next(response.user);
        localStorage.setItem(this.USER_KEY, JSON.stringify(response.user));
      } else if (response === null) {
        this.clearAuth();
      }
    });
  }

  /**
   * Registrar nuevo usuario
   */
  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => {
        if (response.success && response.token && response.user) {
          this.setAuth(response.token, response.user);
        }
      })
    );
  }

  /**
   * Iniciar sesión
   */
  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        if (response.success && response.token && response.user) {
          this.setAuth(response.token, response.user);
        }
      })
    );
  }

  /**
   * Cerrar sesión
   */
  logout(): void {
    this.clearAuth();
    this.router.navigate(['/login']);
  }

  /**
   * Cambiar contraseña
   */
  changePassword(request: ChangePasswordRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/change-password`, request);
  }

  /**
   * Obtener todos los usuarios (para asignaciones)
   */
  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/users`);
  }

  /**
   * Guardar autenticación
   */
  private setAuth(token: string, user: User): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this.currentUserSubject.next(user);
    this.isAuthenticatedSubject.next(true);
  }

  /**
   * Limpiar autenticación
   */
  private clearAuth(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }

  /**
   * Obtener token actual
   */
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  /**
   * Obtener usuario actual
   */
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Verificar si está autenticado
   */
  isLoggedIn(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  /**
   * Obtener nombre del usuario actual
   */
  getCurrentUserName(): string {
    return this.currentUserSubject.value?.name || 'Usuario';
  }

  /**
   * Obtener email del usuario actual
   */
  getCurrentUserEmail(): string {
    return this.currentUserSubject.value?.email || '';
  }
}
