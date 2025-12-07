import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TemplateService } from '../../services/template.service';
import { ProjectService } from '../../services/project.service';
import { OpenUpTemplate, TemplateComparison, CreateTemplateRequest } from '../../models/template.model';
import { ProjectListItem } from '../../models/project.model';

@Component({
  selector: 'app-templates',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './templates.component.html',
  styleUrls: ['./templates.component.scss']
})
export class TemplatesComponent implements OnInit {
  templates: OpenUpTemplate[] = [];
  projects: ProjectListItem[] = [];
  loading = true;
  error: string | null = null;
  successMessage: string | null = null;

  // Modal states
  showCreateModal = false;
  showCompareModal = false;
  showApplyModal = false;
  showVersionModal = false;

  // Form data
  newTemplate: CreateTemplateRequest = {
    name: '',
    description: '',
    version: '1.0',
    configurationJson: '{}',
    isDefault: false,
    createdBy: '',
    phases: []
  };

  // Compare
  compareId1: number | null = null;
  compareId2: number | null = null;
  comparison: TemplateComparison | null = null;

  // Apply
  selectedTemplateId: number | null = null;
  selectedProjectId: number | null = null;

  // Version
  versionTemplateId: number | null = null;
  newVersionNumber: string = '';

  // Phase form
  newPhaseName: string = '';
  newPhaseOrder: number = 1;

  constructor(
    private templateService: TemplateService,
    private projectService: ProjectService
  ) {}

  ngOnInit(): void {
    this.loadTemplates();
    this.loadProjects();
  }

  private loadTemplates(): void {
    this.loading = true;
    this.templateService.getAllTemplates().subscribe({
      next: (data) => {
        this.templates = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar plantillas';
        this.loading = false;
        console.error(err);
      }
    });
  }

  private loadProjects(): void {
    this.projectService.getAllProjects().subscribe({
      next: (data) => this.projects = data,
      error: (err) => console.error('Error loading projects:', err)
    });
  }

  // ==================== CREATE TEMPLATE ====================

  openCreateModal(): void {
    this.newTemplate = {
      name: '',
      description: '',
      version: '1.0',
      configurationJson: '{}',
      isDefault: false,
      createdBy: '',
      phases: []
    };
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
  }

  addPhase(): void {
    if (!this.newPhaseName.trim()) return;
    this.newTemplate.phases.push({
      name: this.newPhaseName,
      order: this.newPhaseOrder,
      mandatoryArtifactTypesJson: '[]'
    });
    this.newPhaseName = '';
    this.newPhaseOrder = this.newTemplate.phases.length + 1;
  }

  removePhase(index: number): void {
    this.newTemplate.phases.splice(index, 1);
  }

  createTemplate(): void {
    if (!this.newTemplate.name.trim()) {
      this.showError('El nombre es requerido');
      return;
    }

    this.templateService.createTemplate(this.newTemplate).subscribe({
      next: () => {
        this.showSuccess('Plantilla creada exitosamente');
        this.loadTemplates();
        this.closeCreateModal();
      },
      error: (err) => this.showError(err.error?.message || 'Error al crear plantilla')
    });
  }

  // ==================== SET DEFAULT ====================

  setAsDefault(template: OpenUpTemplate): void {
    this.templateService.setAsDefault(template.id).subscribe({
      next: () => {
        this.showSuccess(`"${template.name}" es ahora la plantilla predeterminada`);
        this.loadTemplates();
      },
      error: (err) => this.showError(err.error?.message || 'Error al establecer como predeterminada')
    });
  }

  // ==================== DELETE ====================

  deleteTemplate(template: OpenUpTemplate): void {
    if (template.isDefault) {
      this.showError('No se puede eliminar la plantilla predeterminada');
      return;
    }
    
    if (!confirm(`¿Eliminar la plantilla "${template.name}"?`)) return;

    this.templateService.deleteTemplate(template.id).subscribe({
      next: () => {
        this.showSuccess('Plantilla eliminada');
        this.loadTemplates();
      },
      error: (err) => this.showError(err.error?.message || 'Error al eliminar')
    });
  }

  // ==================== COMPARE ====================

  openCompareModal(): void {
    this.compareId1 = null;
    this.compareId2 = null;
    this.comparison = null;
    this.showCompareModal = true;
  }

  closeCompareModal(): void {
    this.showCompareModal = false;
    this.comparison = null;
  }

  compareTemplates(): void {
    if (!this.compareId1 || !this.compareId2) {
      this.showError('Seleccione dos plantillas para comparar');
      return;
    }

    this.templateService.compareTemplates(this.compareId1, this.compareId2).subscribe({
      next: (result) => this.comparison = result,
      error: (err) => this.showError(err.error?.message || 'Error al comparar')
    });
  }

  // ==================== APPLY TO PROJECT ====================

  openApplyModal(templateId: number): void {
    this.selectedTemplateId = templateId;
    this.selectedProjectId = null;
    this.showApplyModal = true;
  }

  closeApplyModal(): void {
    this.showApplyModal = false;
  }

  applyToProject(): void {
    if (!this.selectedTemplateId || !this.selectedProjectId) {
      this.showError('Seleccione un proyecto');
      return;
    }

    this.templateService.applyToProject(this.selectedTemplateId, this.selectedProjectId).subscribe({
      next: (result) => {
        this.showSuccess(result.message);
        this.closeApplyModal();
      },
      error: (err) => this.showError(err.error?.message || 'Error al aplicar plantilla')
    });
  }

  // ==================== CREATE VERSION ====================

  openVersionModal(templateId: number): void {
    const template = this.templates.find(t => t.id === templateId);
    this.versionTemplateId = templateId;
    this.newVersionNumber = template ? this.incrementVersion(template.version) : '1.1';
    this.showVersionModal = true;
  }

  closeVersionModal(): void {
    this.showVersionModal = false;
  }

  private incrementVersion(version: string): string {
    const parts = version.split('.');
    if (parts.length >= 2) {
      const minor = parseInt(parts[1]) + 1;
      return `${parts[0]}.${minor}`;
    }
    return version + '.1';
  }

  createVersion(): void {
    if (!this.versionTemplateId || !this.newVersionNumber.trim()) {
      this.showError('Ingrese el número de versión');
      return;
    }

    this.templateService.createVersion(this.versionTemplateId, this.newVersionNumber).subscribe({
      next: () => {
        this.showSuccess(`Nueva versión ${this.newVersionNumber} creada`);
        this.loadTemplates();
        this.closeVersionModal();
      },
      error: (err) => this.showError(err.error?.message || 'Error al crear versión')
    });
  }

  // ==================== HELPERS ====================

  private showSuccess(message: string): void {
    this.successMessage = message;
    this.error = null;
    setTimeout(() => this.successMessage = null, 3000);
  }

  private showError(message: string): void {
    this.error = message;
    this.successMessage = null;
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
