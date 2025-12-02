import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DefectService } from '../../services/defect.service';
import { Defect } from '../../models/defect.model';
import { RouterModule } from '@angular/router';
import { PermissionService } from '../../services/permission.service';

@Component({
  selector: 'app-defect-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-container">
      <div class="header-section">
        <div>
          <h1 class="page-title">Control de Calidad</h1>
          <p class="page-subtitle">Gestión y seguimiento de defectos del proyecto</p>
        </div>
        <a *ngIf="canCreate" routerLink="/defects/new" class="btn-primary">
          <span class="icon">+</span> Reportar Defecto
        </a>
      </div>

      <div *ngIf="defects.length === 0" class="empty-state">
        <div class="empty-icon">🐞</div>
        <h3>No hay defectos reportados</h3>
        <p>¡Buen trabajo! O quizás nadie ha probado nada aún...</p>
      </div>

      <div *ngIf="defects.length > 0" class="card-table-container">
        <table class="modern-table">
          <thead>
            <tr>
              <th width="5%">ID</th>
              <th width="35%">Defecto</th>
              <th width="15%">Severidad</th>
              <th width="15%">Estado</th>
              <th width="20%">Asignado a</th>
              <th width="10%">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let d of defects">
              <td class="id-cell">#{{d.id}}</td>
              <td>
                <div class="defect-info">
                  <span class="defect-title">{{d.title}}</span>
                  <span class="defect-desc">{{d.description | slice:0:50}}{{d.description.length > 50 ? '...' : ''}}</span>
                </div>
              </td>
              <td>
                <span [class]="'badge severity-' + d.severity.toLowerCase()">
                  {{d.severity}}
                </span>
              </td>
              <td>
                <div class="status-indicator">
                  <span [class]="'dot ' + d.status.toLowerCase()"></span>
                  {{d.status}}
                </div>
              </td>
              <td class="user-cell">
                <div class="avatar-circle">{{(d.assignedTo || '?').charAt(0)}}</div>
                <span>{{d.assignedTo || 'Sin asignar'}}</span>
              </td>
              <td>
                <button class="btn-icon">✏️</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    /* Layout General */
    .page-container { max-width: 1200px; margin: 0 auto; padding: 2rem; font-family: 'Inter', sans-serif; }
    
    /* Header */
    .header-section { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
    .page-title { font-size: 1.75rem; font-weight: 700; color: #111827; margin: 0; }
    .page-subtitle { color: #6b7280; margin-top: 0.25rem; }

    /* Botón Primario */
    .btn-primary { 
      background-color: #2563eb; color: white; padding: 0.75rem 1.5rem; 
      border-radius: 8px; text-decoration: none; font-weight: 500; 
      transition: all 0.2s; display: flex; align-items: center; gap: 0.5rem;
      box-shadow: 0 4px 6px -1px rgba(37, 99, 235, 0.2);
    }
    .btn-primary:hover { background-color: #1d4ed8; transform: translateY(-1px); }

    /* Tabla Estilizada */
    .card-table-container { 
      background: white; border-radius: 12px; 
      box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px -1px rgba(0, 0, 0, 0.06); 
      overflow: hidden; border: 1px solid #e5e7eb;
    }
    .modern-table { width: 100%; border-collapse: collapse; text-align: left; }
    .modern-table th { 
      background-color: #f9fafb; padding: 1rem 1.5rem; font-size: 0.75rem; 
      text-transform: uppercase; letter-spacing: 0.05em; color: #6b7280; font-weight: 600; 
    }
    .modern-table td { padding: 1rem 1.5rem; border-top: 1px solid #e5e7eb; vertical-align: middle; }
    .modern-table tr:hover { background-color: #f9fafb; }

    /* Celdas Específicas */
    .id-cell { font-family: monospace; color: #6b7280; }
    .defect-info { display: flex; flex-direction: column; }
    .defect-title { font-weight: 500; color: #111827; }
    .defect-desc { font-size: 0.875rem; color: #6b7280; margin-top: 2px; }

    /* Badges de Severidad */
    .badge { padding: 0.25rem 0.75rem; border-radius: 9999px; font-size: 0.75rem; font-weight: 600; }
    .severity-critical { background-color: #fee2e2; color: #991b1b; }
    .severity-high { background-color: #ffedd5; color: #9a3412; }
    .severity-medium { background-color: #fef3c7; color: #92400e; }
    .severity-low { background-color: #d1fae5; color: #065f46; }

    /* Estado */
    .status-indicator { display: flex; align-items: center; gap: 0.5rem; font-size: 0.875rem; color: #374151; }
    .dot { width: 8px; height: 8px; border-radius: 50%; display: inline-block; }
    .dot.new { background-color: #3b82f6; }
    .dot.assigned { background-color: #f59e0b; }
    .dot.fixed { background-color: #10b981; }
    .dot.closed { background-color: #6b7280; }

    /* Avatar */
    .user-cell { display: flex; align-items: center; gap: 0.75rem; font-size: 0.875rem; color: #374151; }
    .avatar-circle { 
      width: 28px; height: 28px; background-color: #e5e7eb; border-radius: 50%; 
      display: flex; align-items: center; justify-content: center; 
      font-size: 0.75rem; font-weight: 600; color: #4b5563;
    }

    /* Estado Vacío */
    .empty-state { text-align: center; padding: 4rem 1rem; color: #6b7280; }
    .empty-icon { font-size: 3rem; margin-bottom: 1rem; opacity: 0.5; }

    .btn-icon { background: none; border: none; cursor: pointer; opacity: 0.6; transition: opacity 0.2s; }
    .btn-icon:hover { opacity: 1; }
  `]
})
export class DefectListComponent implements OnInit {
  defects: Defect[] = [];
  canCreate: boolean = false;

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
      next: (data) => this.defects = data,
      error: (err) => console.error('Error al cargar defectos:', err)
    });
  }
}