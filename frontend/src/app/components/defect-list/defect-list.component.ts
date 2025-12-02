import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DefectService } from '../../services/defect.service';
import { Defect, DefectSeverityLabels, DefectStatusLabels } from '../../models/defect.model';

@Component({
  selector: 'app-defect-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="defects-container">
      <div class="defects-header">
        <div class="header-content">
          <div class="title-section">
            <span class="icon">🐞</span>
            <div>
              <h1>Gestión de Defectos</h1>
              <p class="subtitle">Registra y da seguimiento a los defectos del proyecto</p>
            </div>
          </div>
          <a routerLink="/defects/new" class="btn-new">
            <span class="plus">+</span> Nuevo Defecto
          </a>
        </div>
      </div>

      <div class="defects-content">
        <div class="card">
          <div class="card-body">
            <div *ngIf="loading" class="text-center">
              <div class="spinner-border" role="status">
                <span class="visually-hidden">Cargando...</span>
              </div>
            </div>

            <div *ngIf="!loading && defects.length === 0" class="empty-state">
              <span class="empty-icon">📋</span>
              <h3>No hay defectos registrados</h3>
              <p>Comienza registrando un nuevo defecto para dar seguimiento</p>
              <a routerLink="/defects/new" class="btn-empty">+ Registrar Defecto</a>
            </div>

            <div *ngIf="!loading && defects.length > 0" class="table-responsive">
              <table class="table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Título</th>
                    <th>Severidad</th>
                    <th>Estado</th>
                    <th>Reportado por</th>
                    <th>Asignado a</th>
                    <th>Fecha</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let defect of defects">
                    <td class="id-cell">#{{defect.id}}</td>
                    <td class="title-cell">{{defect.title}}</td>
                    <td>
                      <span [class]="getSeverityBadgeClass(defect.severity)">
                        {{getSeverityLabel(defect.severity)}}
                      </span>
                    </td>
                    <td>
                      <span [class]="getStatusBadgeClass(defect.status)">
                        {{getStatusLabel(defect.status)}}
                      </span>
                    </td>
                    <td>{{defect.reportedBy}}</td>
                    <td>{{defect.assignedTo || '-'}}</td>
                    <td class="date-cell">{{defect.createdAt | date:'dd/MM/yyyy'}}</td>
                  </tr>
                </tbody>
              </table>
            </div>
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
    .btn-new .plus {
      font-size: 1.25rem;
      font-weight: bold;
    }
    .defects-content {
      max-width: 1200px;
      margin: -2rem auto 2rem auto;
      padding: 0 1rem;
    }
    .card {
      background: white;
      border: none;
      border-radius: 16px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.08);
      overflow: hidden;
    }
    .card-body {
      padding: 0;
    }
    .table {
      margin-bottom: 0;
      width: 100%;
    }
    .table th {
      background-color: #f8fafc;
      font-weight: 600;
      color: #475569;
      border-bottom: 2px solid #e2e8f0;
      padding: 1rem 1.25rem;
      font-size: 0.8rem;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .table td {
      padding: 1rem 1.25rem;
      vertical-align: middle;
      color: #334155;
      border-bottom: 1px solid #f1f5f9;
    }
    .table tbody tr {
      transition: all 0.2s ease;
    }
    .table tbody tr:hover {
      background-color: #f8fafc;
    }
    .id-cell {
      font-weight: 600;
      color: #6366f1;
    }
    .title-cell {
      font-weight: 500;
      max-width: 250px;
    }
    .date-cell {
      color: #64748b;
      font-size: 0.9rem;
    }
    .badge {
      font-size: 0.75rem;
      padding: 0.45rem 0.85rem;
      border-radius: 20px;
      font-weight: 600;
      letter-spacing: 0.3px;
    }
    .empty-state {
      text-align: center;
      padding: 4rem 2rem;
    }
    .empty-state .empty-icon {
      font-size: 4rem;
      display: block;
      margin-bottom: 1rem;
    }
    .empty-state h3 {
      color: #1f2937;
      margin-bottom: 0.5rem;
    }
    .empty-state p {
      color: #6b7280;
      margin-bottom: 1.5rem;
    }
    .btn-empty {
      background: linear-gradient(135deg, #10b981 0%, #059669 100%);
      color: white;
      border: none;
      border-radius: 8px;
      padding: 0.75rem 1.5rem;
      font-weight: 600;
      text-decoration: none;
      display: inline-block;
    }
    .btn-empty:hover {
      color: white;
    }
    .spinner-border {
      width: 3rem;
      height: 3rem;
      color: #10b981;
    }
    .text-center {
      padding: 3rem;
    }
  `]
})
export class DefectListComponent implements OnInit {
  defects: Defect[] = [];
  loading = true;

  constructor(private defectService: DefectService) {}

  ngOnInit(): void {
    this.loadDefects();
  }

  loadDefects(): void {
    this.loading = true;
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
}
