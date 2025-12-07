import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RegisterRequest } from '../../models/auth.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <div class="auth-header">
          <h1>🚀 OpenUP Manager</h1>
          <p>Sistema de Gestión de Proyectos</p>
        </div>

        <form (ngSubmit)="onSubmit()" #registerForm="ngForm">
          <h2>Crear Cuenta</h2>

          <div *ngIf="errorMessage" class="alert alert-error">
            ❌ {{ errorMessage }}
          </div>

          <div *ngIf="successMessage" class="alert alert-success">
            ✅ {{ successMessage }}
          </div>

          <div class="form-group">
            <label for="name">Nombre completo</label>
            <input
              type="text"
              id="name"
              name="name"
              [(ngModel)]="formData.name"
              required
              minlength="2"
              placeholder="Tu nombre"
              #nameInput="ngModel"
            />
            <div *ngIf="nameInput.invalid && nameInput.touched" class="error-text">
              Nombre requerido (mínimo 2 caracteres)
            </div>
          </div>

          <div class="form-group">
            <label for="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              [(ngModel)]="formData.email"
              required
              email
              placeholder="tu@email.com"
              #emailInput="ngModel"
            />
            <div *ngIf="emailInput.invalid && emailInput.touched" class="error-text">
              Email válido requerido
            </div>
          </div>

          <div class="form-group">
            <label for="password">Contraseña</label>
            <input
              type="password"
              id="password"
              name="password"
              [(ngModel)]="formData.password"
              required
              minlength="6"
              placeholder="Mínimo 6 caracteres"
              #passwordInput="ngModel"
            />
            <div *ngIf="passwordInput.invalid && passwordInput.touched" class="error-text">
              Mínimo 6 caracteres
            </div>
          </div>

          <div class="form-group">
            <label for="confirmPassword">Confirmar contraseña</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              [(ngModel)]="formData.confirmPassword"
              required
              placeholder="Repite tu contraseña"
              #confirmInput="ngModel"
            />
            <div *ngIf="formData.password !== formData.confirmPassword && confirmInput.touched" class="error-text">
              Las contraseñas no coinciden
            </div>
          </div>

          <button
            type="submit"
            class="btn btn-primary btn-block"
            [disabled]="loading || registerForm.invalid || formData.password !== formData.confirmPassword"
          >
            {{ loading ? 'Registrando...' : 'Crear cuenta' }}
          </button>
        </form>

        <div class="auth-footer">
          <p>¿Ya tienes cuenta? <a routerLink="/login">Iniciar sesión</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }

    .auth-card {
      background: white;
      border-radius: 16px;
      padding: 40px;
      width: 100%;
      max-width: 400px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.3);
    }

    .auth-header {
      text-align: center;
      margin-bottom: 30px;
    }

    .auth-header h1 {
      margin: 0;
      color: #333;
      font-size: 28px;
    }

    .auth-header p {
      margin: 8px 0 0;
      color: #666;
    }

    h2 {
      margin: 0 0 20px;
      color: #333;
      font-size: 20px;
    }

    .form-group {
      margin-bottom: 20px;
    }

    label {
      display: block;
      margin-bottom: 6px;
      font-weight: 500;
      color: #333;
    }

    input {
      width: 100%;
      padding: 12px 16px;
      border: 2px solid #e0e0e0;
      border-radius: 8px;
      font-size: 16px;
      transition: border-color 0.2s;
      box-sizing: border-box;
    }

    input:focus {
      outline: none;
      border-color: #667eea;
    }

    input.ng-invalid.ng-touched {
      border-color: #dc3545;
    }

    .error-text {
      color: #dc3545;
      font-size: 12px;
      margin-top: 4px;
    }

    .btn {
      padding: 14px 24px;
      border: none;
      border-radius: 8px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }

    .btn-primary {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(102,126,234,0.4);
    }

    .btn-primary:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .btn-block {
      width: 100%;
    }

    .alert {
      padding: 12px 16px;
      border-radius: 8px;
      margin-bottom: 20px;
    }

    .alert-error {
      background: #fee2e2;
      color: #dc2626;
      border: 1px solid #fecaca;
    }

    .alert-success {
      background: #dcfce7;
      color: #16a34a;
      border: 1px solid #bbf7d0;
    }

    .auth-footer {
      text-align: center;
      margin-top: 24px;
      padding-top: 20px;
      border-top: 1px solid #eee;
    }

    .auth-footer p {
      margin: 0;
      color: #666;
    }

    .auth-footer a {
      color: #667eea;
      text-decoration: none;
      font-weight: 600;
    }

    .auth-footer a:hover {
      text-decoration: underline;
    }
  `]
})
export class RegisterComponent {
  formData: RegisterRequest = {
    name: '',
    email: '',
    password: '',
    confirmPassword: ''
  };

  loading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    // Si ya está logueado, redirigir al home
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/']);
    }
  }

  onSubmit(): void {
    if (this.formData.password !== this.formData.confirmPassword) {
      this.errorMessage = 'Las contraseñas no coinciden';
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.register(this.formData).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success) {
          this.router.navigate(['/']);
        } else {
          this.errorMessage = response.message;
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Error al registrar usuario';
      }
    });
  }
}
