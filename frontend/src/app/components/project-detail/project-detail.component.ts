import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProjectService } from '../../services/project.service';
import { Project, PROJECT_STATUS_LABELS, PHASE_STATUS_LABELS, ProjectPlanVersion } from '../../models/project.model';
import { ProjectProgressComponent } from '../project-progress/project-progress.component';
import { ArtifactsManagerComponent } from '../artifacts-manager/artifacts-manager.component';
import { PermissionService, ProjectRole } from '../../services/permission.service';
import { ProjectMembersComponent } from '../project-members/project-members.component';
import { ProjectClosureComponent } from '../project-closure/project-closure.component';
import { ProjectMemberService } from '../../services/project-member.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ProjectProgressComponent, ArtifactsManagerComponent, ProjectMembersComponent, ProjectClosureComponent],
  templateUrl: './project-detail.component.html',
  styleUrls: ['./project-detail.component.scss']
})
export class ProjectDetailComponent implements OnInit {
  project: Project | null = null;
  loading = true;
  error: string | null = null;
  canDelete: boolean = false;
  currentUserProjectRole: string | null = null;

  // Cache para datos parseados del plan (evita ExpressionChangedAfterItHasBeenCheckedError)
  private _cronogramaCache: Array<{ date: string; description: string }> | null = null;
  private _responsablesCache: string[] | null = null;
  private _hitosCache: string[] | null = null;

  // Versiones del plan
  planVersions: ProjectPlanVersion[] = [];
  showVersionsModal = false;
  showSaveVersionModal = false;
  versionObservaciones = '';
  savingVersion = false;
  selectedVersion: ProjectPlanVersion | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private projectService: ProjectService,
    private permService: PermissionService,
    private memberService: ProjectMemberService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const id = +params['id'];
      if (id) {
        this.loadProject(id);
        this.loadUserProjectRole(id);
      }
    });
    this.permService.role$.subscribe(() => {
      this.canDelete = this.permService.canDeleteProject();
    });
  }

  private loadUserProjectRole(projectId: number): void {
    const user = this.authService.getCurrentUser();
    if (user) {
      this.memberService.getCurrentUserRole(projectId, user.email).subscribe({
        next: (role) => {
          this.currentUserProjectRole = role;
          if (role) {
            this.permService.setProjectRole(role as ProjectRole);
          }
        },
        error: () => {
          this.currentUserProjectRole = null;
        }
      });
    }
  }

  private loadProject(id: number): void {
    this.loading = true;
    this.error = null;

    this.projectService.getProjectById(id).subscribe({
      next: (project) => {
        this.project = project;
        this.loading = false;
        // Limpiar cache al cargar nuevo proyecto
        this._cronogramaCache = null;
        this._responsablesCache = null;
        this._hitosCache = null;
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

  // HU-026: Handler para cuando el proyecto se cierra
  onProjectClosed(): void {
    if (this.project) {
      this.loadProject(this.project.id);
    }
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

  // Métodos para parsear datos estructurados del plan (con cache)
  parseCronograma(cronogramaJson: string | null): Array<{ date: string; description: string }> {
    if (this._cronogramaCache !== null) return this._cronogramaCache;
    
    if (!cronogramaJson) {
      this._cronogramaCache = [];
      return this._cronogramaCache;
    }
    try {
      const parsed = JSON.parse(cronogramaJson);
      this._cronogramaCache = Array.isArray(parsed) ? parsed : [];
      return this._cronogramaCache;
    } catch {
      this._cronogramaCache = [];
      return this._cronogramaCache;
    }
  }

  parseResponsables(responsablesJson: string | null): string[] {
    if (this._responsablesCache !== null) return this._responsablesCache;
    
    if (!responsablesJson) {
      this._responsablesCache = [];
      return this._responsablesCache;
    }
    try {
      const parsed = JSON.parse(responsablesJson);
      this._responsablesCache = Array.isArray(parsed) ? parsed : [];
      return this._responsablesCache;
    } catch {
      this._responsablesCache = [];
      return this._responsablesCache;
    }
  }

  parseHitos(hitosJson: string | null): string[] {
    if (this._hitosCache !== null) return this._hitosCache;
    
    if (!hitosJson) {
      this._hitosCache = [];
      return this._hitosCache;
    }
    try {
      const parsed = JSON.parse(hitosJson);
      this._hitosCache = Array.isArray(parsed) ? parsed : [];
      return this._hitosCache;
    } catch {
      this._hitosCache = [];
      return this._hitosCache;
    }
  }

  exportToPDF(): void {
    if (!this.project) return;
    
    this.projectService.exportProjectPlanToPdf(this.project.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Plan_${this.project!.code}_${new Date().toISOString().split('T')[0]}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Error al exportar PDF:', err);
        alert('Error al exportar el PDF. Por favor, intente nuevamente.');
      }
    });
  }

  // Métodos para manejar versiones del plan
  openSaveVersionModal(): void {
    this.showSaveVersionModal = true;
    this.versionObservaciones = '';
  }

  closeSaveVersionModal(): void {
    this.showSaveVersionModal = false;
    this.versionObservaciones = '';
  }

  saveNewVersion(): void {
    if (!this.project) return;
    
    if (!this.versionObservaciones.trim()) {
      alert('Por favor ingrese observaciones para esta versión.');
      return;
    }

    this.savingVersion = true;
    
    this.projectService.savePlanVersion(this.project.id, {
      observaciones: this.versionObservaciones
    }).subscribe({
      next: (version) => {
        alert(`Versión ${version.version} guardada exitosamente.`);
        this.closeSaveVersionModal();
        this.loadPlanVersions();
      },
      error: (err) => {
        alert('Error al guardar la versión del plan.');
        console.error('Error:', err);
        this.savingVersion = false;
      }
    });
  }

  openVersionsModal(): void {
    if (!this.project) return;
    this.showVersionsModal = true;
    this.loadPlanVersions();
  }

  closeVersionsModal(): void {
    this.showVersionsModal = false;
    this.selectedVersion = null;
  }

  loadPlanVersions(): void {
    if (!this.project) return;
    
    this.projectService.getPlanVersions(this.project.id).subscribe({
      next: (versions) => {
        this.planVersions = versions;
        this.savingVersion = false;
      },
      error: (err) => {
        console.error('Error al cargar versiones:', err);
        this.savingVersion = false;
      }
    });
  }

  viewVersion(version: ProjectPlanVersion): void {
    this.selectedVersion = version;
  }

  closeVersionDetail(): void {
    this.selectedVersion = null;
  }

  updatePhaseStatus(phase: any, newStatus: string): void {
    if (!confirm(`¿Estás seguro de cambiar el estado de la fase a ${this.getPhaseStatusLabel(newStatus)}?`)) {
      return;
    }

    this.projectService.updatePhaseStatus(phase.id, newStatus).subscribe({
      next: () => {
        phase.status = newStatus;
      },
      error: (err) => {
        console.error('Error actualizando fase:', err);
        alert('Error al actualizar el estado de la fase');
      }
    });
  }
}
