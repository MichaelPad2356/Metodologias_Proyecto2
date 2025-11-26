import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProjectService } from '../../services/project.service';
import { Project, PROJECT_STATUS_LABELS, PHASE_STATUS_LABELS } from '../../models/project.model';
import { ArtifactsManagerComponent } from '../artifacts-manager/artifacts-manager.component'; 

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ArtifactsManagerComponent],
  templateUrl: './project-detail.component.html',
  styleUrls: ['./project-detail.component.scss']
})
export class ProjectDetailComponent implements OnInit {
  project: Project | null = null;
  loading = true;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private projectService: ProjectService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const id = +params['id'];
      if (id) {
        this.loadProject(id);
      }
    });
  }

  private loadProject(id: number): void {
    this.loading = true;
    this.error = null;

    this.projectService.getProjectById(id).subscribe({
      next: (project) => {
        this.project = project;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al cargar el proyecto';
        this.loading = false;
        console.error('Error:', err);
      }
    });
  }

  getStatusLabel(status: string): string {
    return PROJECT_STATUS_LABELS[status as keyof typeof PROJECT_STATUS_LABELS] || status;
  }

  getPhaseStatusLabel(status: string): string {
    return PHASE_STATUS_LABELS[status as keyof typeof PHASE_STATUS_LABELS] || status;
  }

  getStatusClass(status: string): string {
    const classMap: Record<string, string> = {
      'Created': 'status-created',
      'Active': 'status-active',
      'Archived': 'status-archived',
      'Closed': 'status-closed'
    };
    return classMap[status] || '';
  }

  getPhaseStatusClass(status: string): string {
    const classMap: Record<string, string> = {
      'NotStarted': 'phase-not-started',
      'InProgress': 'phase-in-progress',
      'Completed': 'phase-completed'
    };
    return classMap[status] || '';
  }

  confirmArchive(): void {
    if (!this.project) return;

    if (confirm(`¿Está seguro de archivar el proyecto "${this.project.name}"?`)) {
      this.archiveProject();
    }
  }

  private archiveProject(): void {
    if (!this.project) return;

    this.projectService.archiveProject(this.project.id).subscribe({
      next: () => {
        this.router.navigate(['/projects']);
      },
      error: (err) => {
        alert('Error al archivar el proyecto');
        console.error('Error:', err);
      }
    });
  }

  confirmUnarchive(): void {
    if (!this.project) return;

    if (confirm(`¿Está seguro de desarchivar el proyecto "${this.project.name}"?`)) {
      this.unarchiveProject();
    }
  }

  private unarchiveProject(): void {
    if (!this.project) return;

    this.projectService.unarchiveProject(this.project.id).subscribe({
      next: () => {
        // Recargar el proyecto para mostrar el cambio de estado
        if (this.project) {
          this.loadProject(this.project.id);
        }
      },
      error: (err) => {
        alert('Error al desarchivar el proyecto');
        console.error('Error:', err);
      }
    });
  }

  confirmDelete(): void {
    if (!this.project) return;

    const confirmation = confirm(
      `¿Está COMPLETAMENTE SEGURO de eliminar permanentemente el proyecto "${this.project.name}"?\n\n` +
      `Esta acción NO se puede deshacer y se perderán todos los datos asociados, incluyendo:\n` +
      `- Todas las fases\n` +
      `- Todos los artefactos\n` +
      `- Todos los registros de auditoría\n\n` +
      `Escriba "ELIMINAR" para confirmar.`
    );

    if (confirmation) {
      const secondConfirm = prompt('Escriba "ELIMINAR" para confirmar:');
      if (secondConfirm === 'ELIMINAR') {
        this.deleteProject();
      }
    }
  }

  private deleteProject(): void {
    if (!this.project) return;

    this.projectService.deleteProject(this.project.id).subscribe({
      next: () => {
        this.router.navigate(['/projects']);
      },
      error: (err) => {
        alert('Error al eliminar el proyecto');
        console.error('Error:', err);
      }
    });
  }
}
