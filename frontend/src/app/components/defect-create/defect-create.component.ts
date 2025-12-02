import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { DefectService } from '../../services/defect.service';
import { ProjectService } from '../../services/project.service';
import { ArtifactService } from '../../services/artifactService';
import { Defect, DefectSeverity, DefectStatus } from '../../models/defect.model';
import { PermissionService } from '../../services/permission.service';
import { ProjectListItem } from '../../models/project.model';
import { Artifact, ArtifactType } from '../../models/artifact.model';

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
            <label>Proyecto</label>
            <select [(ngModel)]="defect.projectId" name="projectId" class="form-select" (change)="onProjectChange()" required>
              <option [ngValue]="0" disabled>Seleccione un proyecto...</option>
              <option *ngFor="let p of projects" [ngValue]="p.id">{{ p.name }}</option>
            </select>
          </div>

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
          <a routerLink="/defects" class="btn-back">
            ← Volver a la lista
          </a>
        </div>
      </div>

          <div class="form-group">
            <label>Artefacto Relacionado (Opcional)</label>
            <select [(ngModel)]="defect.artifactId" name="artifactId" class="form-select" [disabled]="!defect.projectId">
              <option [ngValue]="null">-- Ninguno --</option>
              <option *ngFor="let a of artifacts" [ngValue]="a.id">
                {{ getArtifactTypeName(a.type) }} (ID: {{ a.id }}) - {{ a.statusName }}
              </option>
            </select>
            <small *ngIf="!defect.projectId" class="text-muted">Seleccione un proyecto primero</small>
          </div>

          <div class="form-actions">
            <a routerLink="/defects" class="btn-secondary">Cancelar</a>
            <button type="submit" [disabled]="!defectForm.form.valid || isSubmitting" class="btn-primary">
              {{ isSubmitting ? 'Guardando...' : 'Registrar Defecto' }}
            </button>
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
  defect: Defect = {
    title: '',
    description: '',
    severity: DefectSeverity.Medium,
    status: DefectStatus.New,
    projectId: 0, 
    reportedBy: 'Tester'
  };

  projects: ProjectListItem[] = [];
  artifacts: Artifact[] = [];
  canCreate: boolean = false;
  isSubmitting: boolean = false;

  constructor(
    private defectService: DefectService, 
    private projectService: ProjectService,
    private artifactService: ArtifactService,
    private router: Router,
    private permService: PermissionService
  ) {}

  ngOnInit(): void {
    this.loadProjects();
    // Suscribirse a cambios de rol para actualizar permisos en tiempo real
    this.permService.role$.subscribe(() => {
      this.canCreate = this.permService.canCreateDefect();
    });
  }

  loadProjects() {
    this.projectService.getAllProjects().subscribe(data => {
      this.projects = data;
      // Si solo hay un proyecto, seleccionarlo por defecto
      if (this.projects.length === 1) {
        this.defect.projectId = this.projects[0].id;
        this.onProjectChange();
      }
    });
  }

  onProjectChange() {
    if (this.defect.projectId) {
      this.artifactService.getArtifactsByProject(this.defect.projectId).subscribe(data => {
        this.artifacts = data;
      });
    } else {
      this.artifacts = [];
    }
  }

  getArtifactTypeName(type: any): string {
    return ArtifactType[type] || type;
  }

  onSubmit() {
    if (!this.defect.projectId) {
      alert('Seleccione un proyecto');
      return;
    }
    
    console.log('Intentando enviar defecto:', this.defect);
    this.isSubmitting = true;
    
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
