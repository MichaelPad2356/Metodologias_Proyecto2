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
    <div class="container mt-4">
      <div class="row justify-content-center">
        <div class="col-md-8">
          <div class="card">
            <div class="card-header">
              <h4><i class="bi bi-bug me-2"></i>Registrar Nuevo Defecto</h4>
            </div>
            <div class="card-body">
              <form (ngSubmit)="onSubmit()" #defectForm="ngForm">
                <div class="mb-3">
                  <label for="title" class="form-label">Título *</label>
                  <input type="text" class="form-control" id="title" 
                         [(ngModel)]="defect.title" name="title" required>
                </div>

                <div class="mb-3">
                  <label for="description" class="form-label">Descripción *</label>
                  <textarea class="form-control" id="description" rows="4"
                            [(ngModel)]="defect.description" name="description" required></textarea>
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label for="severity" class="form-label">Severidad *</label>
                    <select class="form-select" id="severity" 
                            [(ngModel)]="defect.severity" name="severity" required>
                      <option value="Low">Bajo</option>
                      <option value="Medium">Medio</option>
                      <option value="High">Alto</option>
                      <option value="Critical">Crítico</option>
                    </select>
                  </div>

                  <div class="col-md-6 mb-3">
                    <label for="projectId" class="form-label">Proyecto *</label>
                    <select class="form-select" id="projectId" 
                            [(ngModel)]="defect.projectId" name="projectId" required>
                      <option [ngValue]="null">Seleccionar proyecto...</option>
                      <option *ngFor="let project of projects" [ngValue]="project.id">
                        {{project.name}}
                      </option>
                    </select>
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label for="reportedBy" class="form-label">Reportado por *</label>
                    <input type="text" class="form-control" id="reportedBy" 
                           [(ngModel)]="defect.reportedBy" name="reportedBy" required>
                  </div>

                  <div class="col-md-6 mb-3">
                    <label for="assignedTo" class="form-label">Asignado a</label>
                    <input type="text" class="form-control" id="assignedTo" 
                           [(ngModel)]="defect.assignedTo" name="assignedTo">
                  </div>
                </div>

                <div class="d-flex justify-content-end gap-2">
                  <a routerLink="/defects" class="btn btn-secondary">Cancelar</a>
                  <button type="submit" class="btn btn-primary" 
                          [disabled]="!defectForm.valid || submitting">
                    <span *ngIf="submitting" class="spinner-border spinner-border-sm me-2"></span>
                    Guardar Defecto
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
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
    if (!this.defect.projectId) {
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
