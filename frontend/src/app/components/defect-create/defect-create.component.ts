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
    <div class="defects-container">
      <!-- Header -->
      <div class="defects-header">
        <div class="header-content">
          <div class="title-section">
            <span class="icon">🐞</span>
            <div>
              <h1>Nuevo Reporte de Defecto</h1>
              <p class="subtitle">Detalla el incidente encontrado para su seguimiento</p>
            </div>
          </div>
          <a routerLink="/defects" class="btn-back">
            ← Volver a la lista
          </a>
        </div>
      </div>

      <!-- Contenido del formulario -->
      <div class="form-content">
        <div class="card">
          <div class="card-body">
            <!-- Alerta de permisos -->
            <div *ngIf="!canCreate" class="alert-error">
              <span class="alert-icon">🔒</span>
              <div class="alert-content">
                <strong>Acceso Restringido</strong>
                <p>No tienes permisos para crear defectos. Contacta al administrador.</p>
              </div>
            </div>

            <!-- Formulario -->
            <form *ngIf="canCreate" (ngSubmit)="onSubmit()" #defectForm="ngForm">
              
              <!-- Sección: Información del Proyecto -->
              <div class="form-section">
                <h3 class="section-title">
                  <span class="section-icon">📁</span>
                  Información del Proyecto
                </h3>
                
                <div class="form-group">
                  <label class="form-label">Proyecto *</label>
                  <select [(ngModel)]="defect.projectId" name="projectId" class="form-select" (change)="onProjectChange()" required>
                    <option [ngValue]="0" disabled>Seleccione un proyecto...</option>
                    <option *ngFor="let p of projects" [ngValue]="p.id">{{ p.name }}</option>
                  </select>
                </div>

                <div class="form-group">
                  <label class="form-label">Artefacto Relacionado (Opcional)</label>
                  <select [(ngModel)]="defect.artifactId" name="artifactId" class="form-select" [disabled]="!defect.projectId">
                    <option [ngValue]="null">-- Ninguno --</option>
                    <option *ngFor="let a of artifacts" [ngValue]="a.id">
                      {{ getArtifactTypeName(a.type) }} (ID: {{ a.id }}) - {{ a.statusName }}
                    </option>
                  </select>
                  <small *ngIf="!defect.projectId" class="form-hint">Seleccione un proyecto primero</small>
                </div>
              </div>

              <!-- Sección: Detalles del Defecto -->
              <div class="form-section">
                <h3 class="section-title">
                  <span class="section-icon">📝</span>
                  Detalles del Defecto
                </h3>
                
                <div class="form-group">
                  <label class="form-label">Título del Defecto *</label>
                  <input type="text" [(ngModel)]="defect.title" name="title" required 
                         placeholder="Ej: Error al guardar usuario nuevo" class="form-control">
                </div>
                
                <div class="form-group">
                  <label class="form-label">Descripción y Pasos para Reproducir</label>
                  <textarea [(ngModel)]="defect.description" name="desc" rows="5" 
                            placeholder="1. Ingresar al módulo...&#10;2. Hacer clic en...&#10;3. El error aparece cuando..." 
                            class="form-control"></textarea>
                </div>
              </div>

              <!-- Sección: Clasificación -->
              <div class="form-section">
                <h3 class="section-title">
                  <span class="section-icon">🏷️</span>
                  Clasificación y Asignación
                </h3>
                
                <div class="form-row">
                  <div class="form-group">
                    <label class="form-label">Severidad</label>
                    <div class="severity-options">
                      <label class="severity-option" [class.selected]="defect.severity === 'Low'">
                        <input type="radio" [(ngModel)]="defect.severity" name="severity" value="Low">
                        <span class="severity-badge low">
                          <span class="severity-dot"></span>
                          Baja
                        </span>
                        <span class="severity-desc">Cosmético</span>
                      </label>
                      <label class="severity-option" [class.selected]="defect.severity === 'Medium'">
                        <input type="radio" [(ngModel)]="defect.severity" name="severity" value="Medium">
                        <span class="severity-badge medium">
                          <span class="severity-dot"></span>
                          Media
                        </span>
                        <span class="severity-desc">Funcionalidad parcial</span>
                      </label>
                      <label class="severity-option" [class.selected]="defect.severity === 'High'">
                        <input type="radio" [(ngModel)]="defect.severity" name="severity" value="High">
                        <span class="severity-badge high">
                          <span class="severity-dot"></span>
                          Alta
                        </span>
                        <span class="severity-desc">Funcionalidad crítica</span>
                      </label>
                      <label class="severity-option" [class.selected]="defect.severity === 'Critical'">
                        <input type="radio" [(ngModel)]="defect.severity" name="severity" value="Critical">
                        <span class="severity-badge critical">
                          <span class="severity-dot"></span>
                          Crítica
                        </span>
                        <span class="severity-desc">Bloqueante</span>
                      </label>
                    </div>
                  </div>
                  
                  <div class="form-group">
                    <label class="form-label">Asignar a</label>
                    <div class="input-with-icon">
                      <span class="input-icon">👤</span>
                      <input type="text" [(ngModel)]="defect.assignedTo" name="assigned" 
                             placeholder="Nombre del desarrollador" class="form-control with-icon">
                    </div>
                  </div>
                </div>
              </div>

              <!-- Acciones del formulario -->
              <div class="form-actions">
                <a routerLink="/defects" class="btn-cancel">
                  Cancelar
                </a>
                <button type="submit" [disabled]="!defectForm.form.valid || isSubmitting" class="btn-submit">
                  <span *ngIf="isSubmitting" class="spinner"></span>
                  <span *ngIf="!isSubmitting" class="btn-icon">🐞</span>
                  {{ isSubmitting ? 'Guardando...' : 'Registrar Defecto' }}
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
    
    /* Header */
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
      padding: 0.6rem 1.25rem;
      border-radius: 8px;
      background: rgba(255,255,255,0.15);
      transition: all 0.2s ease;
      font-weight: 500;
    }
    .btn-back:hover {
      background: rgba(255,255,255,0.25);
      color: white;
    }

    /* Form Content */
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

    /* Alert */
    .alert-error {
      display: flex;
      align-items: flex-start;
      gap: 1rem;
      padding: 1.25rem;
      background: linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%);
      border: 1px solid #fecaca;
      border-radius: 12px;
      margin-bottom: 1.5rem;
    }
    .alert-icon {
      font-size: 1.5rem;
    }
    .alert-content strong {
      color: #dc2626;
      display: block;
      margin-bottom: 0.25rem;
    }
    .alert-content p {
      color: #7f1d1d;
      margin: 0;
      font-size: 0.9rem;
    }

    /* Form Sections */
    .form-section {
      margin-bottom: 2rem;
      padding-bottom: 1.5rem;
      border-bottom: 1px solid #e5e7eb;
    }
    .form-section:last-of-type {
      border-bottom: none;
    }
    .section-title {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 1.1rem;
      font-weight: 600;
      color: #1e3a5f;
      margin-bottom: 1.25rem;
    }
    .section-icon {
      font-size: 1.25rem;
    }

    /* Form Groups */
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
      border-color: #3b82f6;
      background-color: white;
      box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
    }
    .form-control::placeholder {
      color: #9ca3af;
    }
    .form-control:disabled, .form-select:disabled {
      background-color: #f3f4f6;
      cursor: not-allowed;
    }
    textarea.form-control {
      resize: vertical;
      min-height: 120px;
      line-height: 1.5;
    }
    .form-hint {
      display: block;
      font-size: 0.8rem;
      color: #6b7280;
      margin-top: 0.35rem;
    }

    /* Input with icon */
    .input-with-icon {
      position: relative;
    }
    .input-icon {
      position: absolute;
      left: 1rem;
      top: 50%;
      transform: translateY(-50%);
      font-size: 1.1rem;
    }
    .form-control.with-icon {
      padding-left: 2.75rem;
    }

    /* Severity Options */
    .severity-options {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 0.75rem;
    }
    @media (max-width: 500px) {
      .severity-options {
        grid-template-columns: 1fr;
      }
    }
    .severity-option {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      padding: 0.85rem 1rem;
      border: 2px solid #e5e7eb;
      border-radius: 10px;
      cursor: pointer;
      transition: all 0.2s ease;
      background: #fafafa;
    }
    .severity-option:hover {
      border-color: #d1d5db;
      background: white;
    }
    .severity-option.selected {
      background: white;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
    }
    .severity-option input {
      display: none;
    }
    .severity-badge {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-weight: 600;
      font-size: 0.9rem;
    }
    .severity-dot {
      width: 10px;
      height: 10px;
      border-radius: 50%;
    }
    .severity-desc {
      font-size: 0.75rem;
      color: #6b7280;
      margin-top: 0.25rem;
    }
    
    /* Severity Colors */
    .severity-option.selected:has(input[value="Low"]) {
      border-color: #10b981;
    }
    .severity-badge.low .severity-dot {
      background: #10b981;
    }
    .severity-badge.low {
      color: #059669;
    }
    
    .severity-option.selected:has(input[value="Medium"]) {
      border-color: #f59e0b;
    }
    .severity-badge.medium .severity-dot {
      background: #f59e0b;
    }
    .severity-badge.medium {
      color: #d97706;
    }
    
    .severity-option.selected:has(input[value="High"]) {
      border-color: #f97316;
    }
    .severity-badge.high .severity-dot {
      background: #f97316;
    }
    .severity-badge.high {
      color: #ea580c;
    }
    
    .severity-option.selected:has(input[value="Critical"]) {
      border-color: #ef4444;
    }
    .severity-badge.critical .severity-dot {
      background: #ef4444;
    }
    .severity-badge.critical {
      color: #dc2626;
    }

    /* Form Actions */
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
      border: none;
      cursor: pointer;
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
      box-shadow: 0 4px 15px rgba(16, 185, 129, 0.3);
    }
    .btn-submit:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(16, 185, 129, 0.4);
    }
    .btn-submit:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none;
      box-shadow: none;
    }
    .btn-icon {
      font-size: 1.1rem;
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
    id: 0,
    createdAt: new Date(),
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
        this.isSubmitting = false;
      }
    });
  }
}
