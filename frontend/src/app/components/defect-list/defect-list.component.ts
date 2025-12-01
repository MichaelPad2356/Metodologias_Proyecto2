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
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2><i class="bi bi-bug me-2"></i>Gestión de Defectos</h2>
        <a routerLink="/defects/new" class="btn btn-primary">
          <i class="bi bi-plus-circle me-2"></i>Nuevo Defecto
        </a>
      </div>

      <div class="card">
        <div class="card-body">
          <div *ngIf="loading" class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Cargando...</span>
            </div>
          </div>

          <div *ngIf="!loading && defects.length === 0" class="alert alert-info">
            <i class="bi bi-info-circle me-2"></i>No hay defectos registrados.
          </div>

          <div *ngIf="!loading && defects.length > 0" class="table-responsive">
            <table class="table table-hover">
              <thead class="table-light">
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
                  <td>{{defect.id}}</td>
                  <td>{{defect.title}}</td>
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
                  <td>{{defect.createdAt | date:'short'}}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .badge { font-size: 0.85rem; }
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
