import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { InvitationService, UserInvitation } from '../../services/invitation.service';

@Component({
  selector: 'app-invitations',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="invitations-container">
      <div class="header">
        <h1>📬 Mis Invitaciones</h1>
        <p class="subtitle">Gestiona las invitaciones a proyectos que has recibido</p>
      </div>

      <!-- Invitaciones Pendientes -->
      <section class="section" *ngIf="pendingInvitations.length > 0">
        <h2>🔔 Invitaciones Pendientes ({{ pendingInvitations.length }})</h2>
        <div class="invitations-grid">
          <div class="invitation-card pending" *ngFor="let inv of pendingInvitations">
            <div class="project-info">
              <span class="project-code">{{ inv.projectCode }}</span>
              <h3>{{ inv.projectName }}</h3>
              <p class="role">
                <span class="role-badge">{{ inv.role }}</span>
              </p>
              <p class="meta">
                <span>Invitado por: {{ inv.invitedBy || 'Sistema' }}</span>
                <span>{{ formatDate(inv.invitedAt) }}</span>
              </p>
            </div>
            <div class="actions">
              <button class="btn-accept" (click)="acceptInvitation(inv)" [disabled]="processing">
                ✅ Aceptar
              </button>
              <button class="btn-decline" (click)="declineInvitation(inv)" [disabled]="processing">
                ❌ Rechazar
              </button>
            </div>
          </div>
        </div>
      </section>

      <!-- Sin invitaciones pendientes -->
      <div class="empty-state" *ngIf="pendingInvitations.length === 0 && !loading">
        <div class="empty-icon">📭</div>
        <h3>No tienes invitaciones pendientes</h3>
        <p>Cuando alguien te invite a un proyecto, aparecerá aquí</p>
        <a routerLink="/projects" class="btn-primary">Ver mis proyectos</a>
      </div>

      <!-- Loading -->
      <div class="loading" *ngIf="loading">
        <div class="spinner"></div>
        <p>Cargando invitaciones...</p>
      </div>

      <!-- Historial de invitaciones -->
      <section class="section history" *ngIf="allInvitations.length > 0 && !loading">
        <h2>📋 Historial de Invitaciones</h2>
        <table class="invitations-table">
          <thead>
            <tr>
              <th>Proyecto</th>
              <th>Rol</th>
              <th>Estado</th>
              <th>Invitado por</th>
              <th>Fecha</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let inv of allInvitations" [class]="inv.status?.toLowerCase()">
              <td>
                <strong>{{ inv.projectName }}</strong>
                <span class="code">{{ inv.projectCode }}</span>
              </td>
              <td>{{ inv.role }}</td>
              <td>
                <span class="status-badge" [class]="inv.status?.toLowerCase()">
                  {{ getStatusLabel(inv.status) }}
                </span>
              </td>
              <td>{{ inv.invitedBy || 'Sistema' }}</td>
              <td>{{ formatDate(inv.invitedAt) }}</td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- Mensaje de éxito/error -->
      <div class="toast" *ngIf="message" [class.success]="!isError" [class.error]="isError">
        {{ message }}
      </div>
    </div>
  `,
  styles: [`
    .invitations-container {
      max-width: 1000px;
      margin: 0 auto;
      padding: 2rem;
    }

    .header {
      margin-bottom: 2rem;
    }

    .header h1 {
      font-size: 2rem;
      color: #1f2937;
      margin-bottom: 0.5rem;
    }

    .subtitle {
      color: #6b7280;
    }

    .section {
      margin-bottom: 2rem;
    }

    .section h2 {
      font-size: 1.25rem;
      color: #374151;
      margin-bottom: 1rem;
      padding-bottom: 0.5rem;
      border-bottom: 2px solid #e5e7eb;
    }

    .invitations-grid {
      display: grid;
      gap: 1rem;
    }

    .invitation-card {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-left: 4px solid #667eea;
    }

    .invitation-card.pending {
      border-left-color: #f59e0b;
      background: linear-gradient(to right, #fffbeb, white);
    }

    .project-info h3 {
      margin: 0.25rem 0;
      color: #1f2937;
    }

    .project-code {
      font-size: 0.75rem;
      color: #667eea;
      font-weight: 600;
      text-transform: uppercase;
    }

    .role-badge {
      background: #eef2ff;
      color: #667eea;
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 0.85rem;
      font-weight: 500;
    }

    .meta {
      display: flex;
      gap: 1rem;
      font-size: 0.85rem;
      color: #6b7280;
      margin-top: 0.5rem;
    }

    .actions {
      display: flex;
      gap: 0.75rem;
    }

    .btn-accept {
      background: #10b981;
      color: white;
      border: none;
      padding: 10px 20px;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.2s;
    }

    .btn-accept:hover:not(:disabled) {
      background: #059669;
      transform: translateY(-2px);
    }

    .btn-decline {
      background: #f3f4f6;
      color: #6b7280;
      border: none;
      padding: 10px 20px;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.2s;
    }

    .btn-decline:hover:not(:disabled) {
      background: #fee2e2;
      color: #dc2626;
    }

    button:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .empty-state {
      text-align: center;
      padding: 4rem 2rem;
      background: white;
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .empty-icon {
      font-size: 4rem;
      margin-bottom: 1rem;
    }

    .empty-state h3 {
      color: #374151;
      margin-bottom: 0.5rem;
    }

    .empty-state p {
      color: #6b7280;
      margin-bottom: 1.5rem;
    }

    .btn-primary {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      padding: 12px 24px;
      border-radius: 8px;
      text-decoration: none;
      font-weight: 500;
      display: inline-block;
    }

    .loading {
      text-align: center;
      padding: 3rem;
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid #e5e7eb;
      border-top-color: #667eea;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 1rem;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .invitations-table {
      width: 100%;
      border-collapse: collapse;
      background: white;
      border-radius: 8px;
      overflow: hidden;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .invitations-table th,
    .invitations-table td {
      padding: 12px 16px;
      text-align: left;
      border-bottom: 1px solid #e5e7eb;
    }

    .invitations-table th {
      background: #f9fafb;
      font-weight: 600;
      color: #374151;
    }

    .invitations-table td .code {
      display: block;
      font-size: 0.75rem;
      color: #6b7280;
    }

    .status-badge {
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 0.8rem;
      font-weight: 500;
    }

    .status-badge.pending {
      background: #fef3c7;
      color: #d97706;
    }

    .status-badge.accepted {
      background: #d1fae5;
      color: #059669;
    }

    .status-badge.declined {
      background: #fee2e2;
      color: #dc2626;
    }

    .toast {
      position: fixed;
      bottom: 20px;
      right: 20px;
      padding: 16px 24px;
      border-radius: 8px;
      color: white;
      font-weight: 500;
      animation: slideIn 0.3s ease;
      z-index: 1000;
    }

    .toast.success {
      background: #10b981;
    }

    .toast.error {
      background: #ef4444;
    }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    .history {
      margin-top: 3rem;
    }
  `]
})
export class InvitationsComponent implements OnInit {
  pendingInvitations: UserInvitation[] = [];
  allInvitations: UserInvitation[] = [];
  loading = true;
  processing = false;
  message = '';
  isError = false;

  constructor(private invitationService: InvitationService) {}

  ngOnInit(): void {
    this.loadInvitations();
  }

  loadInvitations(): void {
    this.loading = true;
    
    // Cargar pendientes
    this.invitationService.getPendingInvitations().subscribe({
      next: (invitations) => {
        this.pendingInvitations = invitations;
      },
      error: (err) => {
        console.error('Error loading pending invitations:', err);
      }
    });

    // Cargar historial
    this.invitationService.getAllInvitations().subscribe({
      next: (invitations) => {
        this.allInvitations = invitations;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading invitations:', err);
        this.loading = false;
      }
    });
  }

  acceptInvitation(invitation: UserInvitation): void {
    this.processing = true;
    this.invitationService.acceptInvitation(invitation.id).subscribe({
      next: (response) => {
        this.showMessage(response.message, false);
        this.loadInvitations();
        this.processing = false;
      },
      error: (err) => {
        this.showMessage(err.error?.message || 'Error al aceptar invitación', true);
        this.processing = false;
      }
    });
  }

  declineInvitation(invitation: UserInvitation): void {
    if (!confirm(`¿Estás seguro de rechazar la invitación al proyecto "${invitation.projectName}"?`)) {
      return;
    }

    this.processing = true;
    this.invitationService.declineInvitation(invitation.id).subscribe({
      next: (response) => {
        this.showMessage(response.message, false);
        this.loadInvitations();
        this.processing = false;
      },
      error: (err) => {
        this.showMessage(err.error?.message || 'Error al rechazar invitación', true);
        this.processing = false;
      }
    });
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('es-ES', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getStatusLabel(status?: string): string {
    const labels: Record<string, string> = {
      'Pending': 'Pendiente',
      'Accepted': 'Aceptada',
      'Declined': 'Rechazada'
    };
    return labels[status || ''] || status || '';
  }

  private showMessage(msg: string, isError: boolean): void {
    this.message = msg;
    this.isError = isError;
    setTimeout(() => {
      this.message = '';
    }, 4000);
  }
}
