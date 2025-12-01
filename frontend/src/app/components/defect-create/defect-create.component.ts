import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { DefectService } from '../../services/defect.service';
import { Defect, DefectSeverity, DefectStatus } from '../../models/defect.model';
import { PermissionService } from '../../services/permission.service';

@Component({
  selector: 'app-defect-create',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="page-container">
      <div class="form-card">
        <div class="form-header">
          <h2>Nuevo Reporte de Defecto</h2>
          <p>Detalla el incidente encontrado para su seguimiento.</p>
        </div>

        <div *ngIf="!canCreate" class="alert-error">
          🔒 No tienes permisos para crear defectos. Contacta al administrador.
        </div>

        <form *ngIf="canCreate" (ngSubmit)="onSubmit()" #defectForm="ngForm">

          <div class="form-group">
            <label>Título del Defecto</label>
            <input type="text" [(ngModel)]="defect.title" name="title" required
                   placeholder="Ej: Error al guardar usuario nuevo" class="form-input">
          </div>

          <div class="form-group">
            <label>Descripción y Pasos para Reproducir</label>
            <textarea [(ngModel)]="defect.description" name="desc" rows="5"
                      placeholder="1. Ingresar al módulo..." class="form-input"></textarea>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Severidad</label>
              <select [(ngModel)]="defect.severity" name="severity" class="form-select">
                <option value="Low">🟢 Baja (Cosmético)</option>
                <option value="Medium">🟡 Media (Funcionalidad parcial)</option>
                <option value="High">🟠 Alta (Funcionalidad crítica)</option>
                <option value="Critical">🔴 Crítica (Bloqueante)</option>
              </select>
            </div>

            <div class="form-group">
              <label>Asignar a (Simulado)</label>
              <input type="text" [(ngModel)]="defect.assignedTo" name="assigned"
                     placeholder="Nombre del desarrollador" class="form-input">
            </div>
          </div>

          <div class="form-actions">
            <a routerLink="/defects" class="btn-secondary">Cancelar</a>
            <button type="submit" [disabled]="!defectForm.form.valid || isSubmitting" class="btn-primary">
              {{ isSubmitting ? 'Guardando...' : 'Registrar Defecto' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .page-container {
      min-height: 90vh; display: flex; justify-content: center; padding: 2rem; background-color: #f3f4f6;
    }
    .form-card {
      background: white; width: 100%; max-width: 600px; padding: 2.5rem;
      border-radius: 16px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1); height: fit-content;
    }
    .form-header { margin-bottom: 2rem; border-bottom: 1px solid #e5e7eb; padding-bottom: 1rem; }
    .form-header h2 { margin: 0; color: #111827; font-size: 1.5rem; font-weight: 700; }
    .form-header p { color: #6b7280; margin-top: 0.5rem; }

    .form-group { margin-bottom: 1.5rem; }
    .form-group label { display: block; font-weight: 500; color: #374151; margin-bottom: 0.5rem; font-size: 0.9rem; }
    .form-input, .form-select {
      width: 100%; padding: 0.75rem; border: 1px solid #d1d5db; border-radius: 8px;
      font-size: 0.95rem; transition: border-color 0.2s, box-shadow 0.2s;
      box-sizing: border-box;
    }
    .form-input:focus, .form-select:focus {
      outline: none; border-color: #2563eb; box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
    }

    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }

    .form-actions { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 2rem; }

    .btn-primary {
      background-color: #2563eb; color: white; padding: 0.75rem 1.5rem; border: none;
      border-radius: 8px; font-weight: 600; cursor: pointer; transition: background 0.2s;
    }
    .btn-primary:disabled { background-color: #93c5fd; cursor: not-allowed; }
    .btn-primary:hover:not(:disabled) { background-color: #1d4ed8; }

    .btn-secondary {
      background-color: white; color: #374151; padding: 0.75rem 1.5rem;
      border: 1px solid #d1d5db; border-radius: 8px; font-weight: 500;
      text-decoration: none; cursor: pointer;
    }
    .btn-secondary:hover { background-color: #f9fafb; }

    .alert-error {
      background-color: #fee2e2; color: #991b1b; padding: 1rem;
      border-radius: 8px; margin-bottom: 1.5rem; font-weight: 500;
    }
  `]
})
export class DefectCreateComponent {
  defect: Defect = {
    title: '',
    description: '',
    severity: DefectSeverity.Medium,
    status: DefectStatus.New,
    projectId: 1,
    reportedBy: 'Tester'
  };

  canCreate: boolean = false;
  isSubmitting: boolean = false;

  constructor(
    private defectService: DefectService,
    private router: Router,
    private permService: PermissionService
  ) {
    this.canCreate = this.permService.canCreateDefect();
  }

  onSubmit() {
    console.log('Intentando enviar defecto:', this.defect);
    this.isSubmitting = true;

    this.defectService.createDefect(this.defect).subscribe({
      next: (res) => {
        console.log('Defecto creado exitosamente:', res);
        this.router.navigate(['/defects']);
      },
      error: (err) => {
        console.error('Error creando defecto:', err);
        this.isSubmitting = false;
        alert('Error al crear. Revisa la consola (F12) para más detalles.');
      }
    });
  }
}
