import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { DefectService } from '../../services/defect.service';
import { CreateDefectDto, DefectSeverity } from '../../models/defect.model';
import { ProjectService } from '../../services/project.service';
import { Project } from '../../models/project.model';

@Component({
  selector: 'app-defect-create',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="defects-container">
      <div class="defects-header">
        <div class="header-content">
          <div class="title-section">
            <span class="icon">🐞</span>
            <div>
              <h1>Registrar Nuevo Defecto</h1>
              <p class="subtitle">Completa la información del defecto encontrado</p>
            </div>
          </div>
          <a routerLink="/defects" class="btn-back">
            ← Volver a la lista
          </a>
        </div>
      </div>

      <div class="form-content">
        <div class="card">
          <div class="card-body">
            <form (ngSubmit)="onSubmit()" #defectForm="ngForm">
              <div class="form-section">
                <h3 class="section-title">📝 Información del Defecto</h3>
                
                <div class="form-group">
                  <label for="title" class="form-label">Título del Defecto *</label>
                  <input type="text" class="form-control" id="title" 
                         placeholder="Ej: Error al guardar formulario de registro"
                         [(ngModel)]="defect.title" name="title" required>
                </div>

                <div class="form-group">
                  <label for="description" class="form-label">Descripción Detallada *</label>
                  <textarea class="form-control" id="description" rows="4"
                            placeholder="Describe el defecto, pasos para reproducirlo y comportamiento esperado..."
                            [(ngModel)]="defect.description" name="description" required></textarea>
                </div>

                <div class="form-row">
                  <div class="form-group">
                    <label for="severity" class="form-label">Severidad *</label>
                    <select class="form-select" id="severity" 
                            [(ngModel)]="defect.severity" name="severity" required>
                      <option value="Low">🟢 Bajo - No afecta funcionalidad principal</option>
                      <option value="Medium">🟡 Medio - Afecta funcionalidad secundaria</option>
                      <option value="High">🟠 Alto - Afecta funcionalidad importante</option>
                      <option value="Critical">🔴 Crítico - Sistema no funciona</option>
                    </select>
                  </div>

                  <div class="form-group">
                    <label for="projectId" class="form-label">Proyecto Asociado *</label>
                    <select class="form-select" id="projectId" 
                            [(ngModel)]="defect.projectId" name="projectId" required>
                      <option [ngValue]="0" disabled>Seleccionar proyecto...</option>
                      <option *ngFor="let project of projects" [ngValue]="project.id">
                        {{project.name}}
                      </option>
                    </select>
                  </div>
                </div>
              </div>

              <div class="form-section">
                <h3 class="section-title">👥 Asignación</h3>
                
                <div class="form-row">
                  <div class="form-group">
                    <label for="reportedBy" class="form-label">Reportado por *</label>
                    <input type="text" class="form-control" id="reportedBy" 
                           placeholder="Nombre de quien reporta"
                           [(ngModel)]="defect.reportedBy" name="reportedBy" required>
                  </div>

                  <div class="form-group">
                    <label for="assignedTo" class="form-label">Asignado a</label>
                    <input type="text" class="form-control" id="assignedTo" 
                           placeholder="Nombre del responsable (opcional)"
                           [(ngModel)]="defect.assignedTo" name="assignedTo">
                  </div>
                </div>
              </div>

              <div class="form-actions">
                <a routerLink="/defects" class="btn-cancel">Cancelar</a>
                <button type="submit" class="btn-submit" 
                        [disabled]="!defectForm.valid || submitting">
                  <span *ngIf="submitting" class="spinner"></span>
                  {{ submitting ? 'Guardando...' : '✓ Guardar Defecto' }}
                </button>
              </div>
            </form>
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
      max-width: 900px;
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
    .btn-back {
      color: white;
      text-decoration: none;
      padding: 0.5rem 1rem;
      border-radius: 8px;
      background: rgba(255,255,255,0.1);
      transition: all 0.2s ease;
    }
    .btn-back:hover {
      background: rgba(255,255,255,0.2);
      color: white;
    }
    .form-content {
      max-width: 900px;
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
      padding: 2rem;
    }
    .form-section {
      margin-bottom: 2rem;
    }
    .section-title {
      font-size: 1.1rem;
      font-weight: 600;
      color: #1e3a5f;
      margin-bottom: 1.25rem;
      padding-bottom: 0.75rem;
      border-bottom: 2px solid #e5e7eb;
    }
    .form-group {
      margin-bottom: 1.25rem;
    }
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
    }
    @media (max-width: 768px) {
      .form-row {
        grid-template-columns: 1fr;
      }
    }
    .form-label {
      display: block;
      font-weight: 500;
      color: #374151;
      margin-bottom: 0.5rem;
      font-size: 0.9rem;
    }
    .form-control, .form-select {
      width: 100%;
      padding: 0.75rem 1rem;
      border: 2px solid #e5e7eb;
      border-radius: 10px;
      font-size: 0.95rem;
      transition: all 0.2s ease;
      background-color: #fafafa;
    }
    .form-control:focus, .form-select:focus {
      outline: none;
      border-color: #10b981;
      background-color: white;
      box-shadow: 0 0 0 4px rgba(16, 185, 129, 0.1);
    }
    .form-control::placeholder {
      color: #9ca3af;
    }
    textarea.form-control {
      resize: vertical;
      min-height: 120px;
    }
    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 1rem;
      margin-top: 2rem;
      padding-top: 1.5rem;
      border-top: 1px solid #e5e7eb;
    }
    .btn-cancel {
      padding: 0.75rem 1.5rem;
      border-radius: 10px;
      font-weight: 500;
      text-decoration: none;
      background-color: #f3f4f6;
      color: #4b5563;
      transition: all 0.2s ease;
    }
    .btn-cancel:hover {
      background-color: #e5e7eb;
      color: #374151;
    }
    .btn-submit {
      padding: 0.75rem 2rem;
      border-radius: 10px;
      font-weight: 600;
      border: none;
      background: linear-gradient(135deg, #10b981 0%, #059669 100%);
      color: white;
      cursor: pointer;
      transition: all 0.3s ease;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .btn-submit:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 4px 15px rgba(16, 185, 129, 0.4);
    }
    .btn-submit:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    .spinner {
      width: 18px;
      height: 18px;
      border: 2px solid white;
      border-top-color: transparent;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `]
})
export class DefectCreateComponent implements OnInit {
  defect: CreateDefectDto = {
    title: '',
    description: '',
    severity: DefectSeverity.Medium,
    projectId: 0,
    reportedBy: ''
  };

  projects: Project[] = [];
  submitting = false;

  constructor(
    private defectService: DefectService,
    private projectService: ProjectService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.projectService.getAllProjects().subscribe({
      next: (projects: any[]) => this.projects = projects,
      error: (err: any) => console.error('Error loading projects:', err)
    });
  }

  onSubmit(): void {
    if (!this.defect.projectId || this.defect.projectId === 0) {
      alert('Por favor seleccione un proyecto');
      return;
    }

    this.submitting = true;
    this.defectService.createDefect(this.defect).subscribe({
      next: () => {
        this.router.navigate(['/defects']);
      },
      error: (err: any) => {
        console.error('Error creating defect:', err);
        alert('Error al crear el defecto');
        this.submitting = false;
      }
    });
  }
}
