import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '../../services/project.service';
import { ProjectListItem, PROJECT_STATUS_LABELS } from '../../models/project.model';
import { PermissionService } from '../../services/permission.service';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.scss']
})
export class ProjectListComponent implements OnInit {
  projects: ProjectListItem[] = [];
  loading = false;
  error: string | null = null;
  includeArchived = false;
  canDelete: boolean = false;

  constructor(
    private projectService: ProjectService,
    private permService: PermissionService
  ) {}

  ngOnInit(): void {
    this.loadProjects();
    this.permService.role$.subscribe(() => {
      this.canDelete = this.permService.canDeleteProject();
    });
  }

  loadProjects(): void {
    this.loading = true;
    this.error = null;

    this.projectService.getAllProjects(this.includeArchived).subscribe({
      next: (projects) => {
        this.projects = projects;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar los proyectos';
        this.loading = false;
        console.error('Error:', err);
      }
    });
  }

  onArchivedToggleChange(): void {
    this.loadProjects();
  }

  getStatusLabel(status: string): string {
    return PROJECT_STATUS_LABELS[status as keyof typeof PROJECT_STATUS_LABELS] || status;
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

  confirmArchive(project: ProjectListItem, event: Event): void {
    event.stopPropagation();
    
    if (confirm(`¿Está seguro de archivar el proyecto "${project.name}"?`)) {
      this.archiveProject(project.id);
    }
  }

  private archiveProject(id: number): void {
    this.loading = true;
    this.projectService.archiveProject(id).subscribe({
      next: () => {
        this.loadProjects();
      },
      error: (err) => {
        this.loading = false;
        alert('Error al archivar el proyecto');
        console.error('Error:', err);
      }
    });
  }

  confirmUnarchive(project: ProjectListItem, event: Event): void {
    event.stopPropagation();
    
    if (confirm(`¿Está seguro de desarchivar el proyecto "${project.name}"?`)) {
      this.unarchiveProject(project.id);
    }
  }

  private unarchiveProject(id: number): void {
    this.loading = true;
    this.projectService.unarchiveProject(id).subscribe({
      next: () => {
        this.loadProjects();
      },
      error: (err) => {
        this.loading = false;
        alert('Error al desarchivar el proyecto');
        console.error('Error:', err);
      }
    });
  }

  confirmDelete(project: ProjectListItem, event: Event): void {
    event.stopPropagation();
    
    const confirmation = confirm(
      `¿Está COMPLETAMENTE SEGURO de eliminar permanentemente el proyecto "${project.name}"?\n\n` +
      `Esta acción NO se puede deshacer y se perderán todos los datos asociados.`
    );
    
    if (confirmation) {
      this.deleteProject(project.id);
    }
  }

  private deleteProject(id: number): void {
    // Remover inmediatamente de la lista en memoria para reflejar el cambio en la UI
    this.projects = this.projects.filter(project => project.id !== id);

    this.loading = true;
    this.projectService.deleteProject(id).subscribe({
      next: () => {
        // Refrescar desde el backend para mantener la lista consistente
        this.loadProjects();
      },
      error: (err) => {
        this.loading = false;
        alert('Error al eliminar el proyecto');
        console.error('Error:', err);
      }
    });
  }
}
