import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PermissionService } from '../../services/permission.service';
import { DefectService } from '../../services/defect.service';
import { Defect, DefectSeverityLabels, DefectStatusLabels } from '../../models/defect.model';

@Component({
  selector: 'app-defect-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="defects-container">
      <!-- Header -->
      <div class="defects-header">
        <div class="header-content">
          <div class="title-section">
            <span class="icon">🐞</span>
            <div>
              <h1>Gestión de Defectos</h1>
              <p class="subtitle">Registra y da seguimiento a los defectos del proyecto</p>
            </div>
          </div>
          <a *ngIf="canCreate" routerLink="/defects/new" class="btn-new">
            <span class="plus-icon">+</span> Reportar Defecto
          </a>
        </div>
      </div>

      <!-- Stats Cards -->
      <div class="stats-section" *ngIf="!loading && defects.length > 0">
        <div class="stats-container">
          <div class="stat-card">
            <div class="stat-icon total">📊</div>
            <div class="stat-info">
              <span class="stat-value">{{ defects.length }}</span>
              <span class="stat-label">Total Defectos</span>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon new">🆕</div>
            <div class="stat-info">
              <span class="stat-value">{{ getCountByStatus('New') }}</span>
              <span class="stat-label">Nuevos</span>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon assigned">👤</div>
            <div class="stat-info">
              <span class="stat-value">{{ getCountByStatus('Assigned') }}</span>
              <span class="stat-label">Asignados</span>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon fixed">✅</div>
            <div class="stat-info">
              <span class="stat-value">{{ getCountByStatus('Fixed') + getCountByStatus('Closed') }}</span>
              <span class="stat-label">Resueltos</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Content -->
      <div class="defects-content">
        <div class="card">
          <!-- Loading -->
          <div *ngIf="loading" class="loading-state">
            <div class="spinner"></div>
            <p>Cargando defectos...</p>
          </div>

          <!-- Empty State -->
          <div *ngIf="!loading && defects.length === 0" class="empty-state">
            <div class="empty-illustration">🔍</div>
            <h3>No hay defectos registrados</h3>
            <p>Comienza registrando un nuevo defecto para dar seguimiento a los problemas del proyecto</p>
            <a *ngIf="canCreate" routerLink="/defects/new" class="btn-empty">
              <span>+</span> Registrar Primer Defecto
            </a>
          </div>

          <!-- Table -->
          <div *ngIf="!loading && defects.length > 0" class="table-container">
            <table class="defects-table">
              <thead>
                <tr>
                  <th class="th-id">ID</th>
                  <th class="th-title">Título</th>
                  <th class="th-severity">Severidad</th>
                  <th class="th-status">Estado</th>
                  <th class="th-reporter">Reportado por</th>
                  <th class="th-assigned">Asignado a</th>
                  <th class="th-date">Fecha</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let defect of defects" class="defect-row">
                  <td class="td-id">
                    <span class="id-badge">#{{ defect.id }}</span>
                  </td>
                  <td class="td-title">
                    <span class="title-text">{{ defect.title }}</span>
                    <span class="description-preview" *ngIf="defect.description">
                      {{ defect.description | slice:0:50 }}{{ defect.description.length > 50 ? '...' : '' }}
                    </span>
                  </td>
                  <td class="td-severity">
                    <span class="severity-badge" [ngClass]="'severity-' + defect.severity.toLowerCase()">
                      <span class="severity-dot"></span>
                      {{ getSeverityLabel(defect.severity) }}
                    </span>
                  </td>
                  <td class="td-status">
                    <span class="status-badge" [ngClass]="'status-' + defect.status.toLowerCase()">
                      {{ getStatusLabel(defect.status) }}
                    </span>
                  </td>
                  <td class="td-reporter">
                    <div class="user-info">
                      <span class="user-avatar">👤</span>
                      <span>{{ defect.reportedBy }}</span>
                    </div>
                  </td>
                  <td class="td-assigned">
                    <div class="user-info" *ngIf="defect.assignedTo">
                      <span class="user-avatar">🛠️</span>
                      <span>{{ defect.assignedTo }}</span>
                    </div>
                    <span class="unassigned" *ngIf="!defect.assignedTo">Sin asignar</span>
                  </td>
                  <td class="td-date">
                    <span class="date-value">{{ defect.createdAt | date:'dd MMM yyyy' }}</span>
                    <span class="time-value">{{ defect.createdAt | date:'HH:mm' }}</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .defects-container {
      min-height: 100vh;
      background: linear-gradient(135deg, #f5f7fa 0%, #e4e8eb 100%);
    }

    /* Header */
    .defects-header {
      background: linear-gradient(135deg, #1e3a5f 0%, #2d5a87 100%);
      padding: 2rem;
      color: white;
    }
    .header-content {
      max-width: 1200px;
      margin: 0 auto;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .title-section {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .title-section .icon {
      font-size: 2.5rem;
    }
    .title-section h1 {
      margin: 0;
      font-size: 1.75rem;
      font-weight: 600;
    }
    .title-section .subtitle {
      margin: 0.25rem 0 0 0;
      opacity: 0.8;
      font-size: 0.9rem;
    }
    .btn-new {
      background: linear-gradient(135deg, #10b981 0%, #059669 100%);
      color: white;
      border: none;
      border-radius: 10px;
      padding: 0.75rem 1.5rem;
      font-weight: 600;
      text-decoration: none;
      display: flex;
      align-items: center;
      gap: 0.5rem;
      transition: all 0.3s ease;
      box-shadow: 0 4px 15px rgba(16, 185, 129, 0.3);
    }
    .btn-new:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(16, 185, 129, 0.4);
      color: white;
    }
    .plus-icon {
      font-size: 1.25rem;
      font-weight: bold;
    }

    /* Stats Section */
    .stats-section {
      max-width: 1200px;
      margin: -1.5rem auto 0 auto;
      padding: 0 1rem;
      position: relative;
      z-index: 10;
    }
    .stats-container {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 1rem;
    }
    @media (max-width: 768px) {
      .stats-container {
        grid-template-columns: repeat(2, 1fr);
      }
    }
    .stat-card {
      background: white;
      border-radius: 12px;
      padding: 1.25rem;
      display: flex;
      align-items: center;
      gap: 1rem;
      box-shadow: 0 4px 15px rgba(0,0,0,0.08);
    }
    .stat-icon {
      width: 50px;
      height: 50px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
    }
    .stat-icon.total { background: linear-gradient(135deg, #e0e7ff 0%, #c7d2fe 100%); }
    .stat-icon.new { background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%); }
    .stat-icon.assigned { background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%); }
    .stat-icon.fixed { background: linear-gradient(135deg, #d1fae5 0%, #a7f3d0 100%); }
    .stat-info {
      display: flex;
      flex-direction: column;
    }
    .stat-value {
      font-size: 1.5rem;
      font-weight: 700;
      color: #1f2937;
    }
    .stat-label {
      font-size: 0.8rem;
      color: #6b7280;
    }

    /* Content */
    .defects-content {
      max-width: 1200px;
      margin: 1.5rem auto 2rem auto;
      padding: 0 1rem;
    }
    .card {
      background: white;
      border: none;
      border-radius: 16px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.08);
      overflow: hidden;
    }

    /* Loading */
    .loading-state {
      text-align: center;
      padding: 4rem 2rem;
    }
    .spinner {
      width: 50px;
      height: 50px;
      border: 4px solid #e5e7eb;
      border-top-color: #3b82f6;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 1rem auto;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
    .loading-state p {
      color: #6b7280;
      margin: 0;
    }

    /* Empty State */
    .empty-state {
      text-align: center;
      padding: 4rem 2rem;
    }
    .empty-illustration {
      font-size: 5rem;
      margin-bottom: 1.5rem;
    }
    .empty-state h3 {
      color: #1f2937;
      margin: 0 0 0.5rem 0;
      font-size: 1.5rem;
    }
    .empty-state p {
      color: #6b7280;
      margin: 0 0 1.5rem 0;
      max-width: 400px;
      margin-left: auto;
      margin-right: auto;
    }
    .btn-empty {
      background: linear-gradient(135deg, #10b981 0%, #059669 100%);
      color: white;
      border: none;
      border-radius: 10px;
      padding: 0.85rem 1.75rem;
      font-weight: 600;
      text-decoration: none;
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      transition: all 0.3s ease;
      box-shadow: 0 4px 15px rgba(16, 185, 129, 0.3);
    }
    .btn-empty:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(16, 185, 129, 0.4);
      color: white;
    }

    /* Table */
    .table-container {
      overflow-x: auto;
    }
    .defects-table {
      width: 100%;
      border-collapse: collapse;
    }
    .defects-table th {
      background: #f8fafc;
      padding: 1rem 1.25rem;
      text-align: left;
      font-weight: 600;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      color: #64748b;
      border-bottom: 2px solid #e2e8f0;
    }
    .defects-table td {
      padding: 1rem 1.25rem;
      border-bottom: 1px solid #f1f5f9;
      vertical-align: middle;
    }
    .defect-row {
      transition: all 0.2s ease;
    }
    .defect-row:hover {
      background: #f8fafc;
    }

    /* Table Cells */
    .td-id {
      width: 80px;
    }
    .id-badge {
      display: inline-block;
      background: linear-gradient(135deg, #e0e7ff 0%, #c7d2fe 100%);
      color: #4f46e5;
      padding: 0.35rem 0.75rem;
      border-radius: 6px;
      font-weight: 600;
      font-size: 0.85rem;
    }
    .td-title {
      max-width: 300px;
    }
    .title-text {
      display: block;
      font-weight: 500;
      color: #1f2937;
      margin-bottom: 0.25rem;
    }
    .description-preview {
      display: block;
      font-size: 0.8rem;
      color: #9ca3af;
    }

    /* Severity Badge */
    .severity-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      padding: 0.4rem 0.85rem;
      border-radius: 20px;
      font-weight: 600;
      font-size: 0.75rem;
    }
    .severity-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
    }
    .severity-low {
      background: #d1fae5;
      color: #065f46;
    }
    .severity-low .severity-dot { background: #10b981; }
    .severity-medium {
      background: #fef3c7;
      color: #92400e;
    }
    .severity-medium .severity-dot { background: #f59e0b; }
    .severity-high {
      background: #ffedd5;
      color: #9a3412;
    }
    .severity-high .severity-dot { background: #f97316; }
    .severity-critical {
      background: #fee2e2;
      color: #991b1b;
    }
    .severity-critical .severity-dot { background: #ef4444; }

    /* Status Badge */
    .status-badge {
      display: inline-block;
      padding: 0.4rem 0.85rem;
      border-radius: 20px;
      font-weight: 600;
      font-size: 0.75rem;
    }
    .status-new {
      background: #dbeafe;
      color: #1e40af;
    }
    .status-assigned {
      background: #fef3c7;
      color: #92400e;
    }
    .status-fixed {
      background: #d1fae5;
      color: #065f46;
    }
    .status-closed {
      background: #e5e7eb;
      color: #374151;
    }

    /* User Info */
    .user-info {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .user-avatar {
      font-size: 1rem;
    }
    .unassigned {
      color: #9ca3af;
      font-style: italic;
      font-size: 0.85rem;
    }

    /* Date Cell */
    .td-date {
      text-align: right;
    }
    .date-value {
      display: block;
      color: #374151;
      font-weight: 500;
    }
    .time-value {
      display: block;
      color: #9ca3af;
      font-size: 0.8rem;
    }
  `]
})
export class DefectListComponent implements OnInit {
  defects: Defect[] = [];
  canCreate: boolean = false;
  loading: boolean = true;

  constructor(
    private defectService: DefectService,
    private permService: PermissionService
  ) {}

  ngOnInit() {
    this.loadDefects();
    this.permService.role$.subscribe(() => {
      this.canCreate = this.permService.canCreateDefect();
    });
  }

  loadDefects() {
    this.defectService.getDefects().subscribe({
      next: (defects: Defect[]) => {
        this.defects = defects;
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Error loading defects:', err);
        this.loading = false;
      }
    });
  }

  getSeverityLabel(severity: string): string {
    return DefectSeverityLabels[severity] || severity;
  }

  getStatusLabel(status: string): string {
    return DefectStatusLabels[status] || status;
  }

  getSeverityBadgeClass(severity: string): string {
    const classes: { [key: string]: string } = {
      'Low': 'badge bg-info',
      'Medium': 'badge bg-warning text-dark',
      'High': 'badge bg-danger',
      'Critical': 'badge bg-dark'
    };
    return classes[severity] || 'badge bg-secondary';
  }

  getStatusBadgeClass(status: string): string {
    const classes: { [key: string]: string } = {
      'New': 'badge bg-primary',
      'Assigned': 'badge bg-warning text-dark',
      'Fixed': 'badge bg-success',
      'Closed': 'badge bg-secondary'
    };
    return classes[status] || 'badge bg-secondary';
  }

  getCountByStatus(status: string): number {
    return this.defects.filter(d => d.status === status).length;
  }
}
