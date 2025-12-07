import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ConfigurationService } from '../../services/configuration.service';
import {
  RoleDefinition,
  StageDefinition,
  ArtifactTypeDefinition,
  CustomFieldDefinition,
  ConfigurationVersion,
  CustomFieldType
} from '../../models/configuration.model';

@Component({
  selector: 'app-configuration',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './configuration.component.html',
  styleUrls: ['./configuration.component.scss']
})
export class ConfigurationComponent implements OnInit {
  // Data
  roles: RoleDefinition[] = [];
  stages: StageDefinition[] = [];
  artifactTypes: ArtifactTypeDefinition[] = [];
  customFields: CustomFieldDefinition[] = [];
  versions: ConfigurationVersion[] = [];

  // UI State
  activeTab: 'roles' | 'stages' | 'artifactTypes' | 'customFields' | 'versions' = 'roles';
  loading = true;
  error: string | null = null;
  successMessage: string | null = null;

  // Modal state
  showModal = false;
  modalMode: 'create' | 'edit' = 'create';
  modalType: 'role' | 'stage' | 'artifactType' | 'customField' | 'version' = 'role';

  // Form data
  formData: any = {};
  fieldTypes = Object.values(CustomFieldType);

  constructor(private configService: ConfigurationService) {}

  ngOnInit(): void {
    this.loadAllData();
  }

  private loadAllData(): void {
    this.loading = true;
    this.loadRoles();
    this.loadStages();
    this.loadArtifactTypes();
    this.loadCustomFields();
    this.loadVersions();
  }

  private loadRoles(): void {
    this.configService.getAllRoles().subscribe({
      next: (data) => {
        this.roles = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading roles:', err);
        this.loading = false;
      }
    });
  }

  private loadStages(): void {
    this.configService.getAllStages().subscribe({
      next: (data) => this.stages = data.sort((a, b) => a.order - b.order),
      error: (err) => console.error('Error loading stages:', err)
    });
  }

  private loadArtifactTypes(): void {
    this.configService.getAllArtifactTypes().subscribe({
      next: (data) => this.artifactTypes = data,
      error: (err) => console.error('Error loading artifact types:', err)
    });
  }

  private loadCustomFields(): void {
    this.configService.getAllCustomFields().subscribe({
      next: (data) => this.customFields = data,
      error: (err) => console.error('Error loading custom fields:', err)
    });
  }

  private loadVersions(): void {
    this.configService.getAllVersions().subscribe({
      next: (data) => this.versions = data.sort((a, b) => b.versionNumber - a.versionNumber),
      error: (err) => console.error('Error loading versions:', err)
    });
  }

  setActiveTab(tab: typeof this.activeTab): void {
    this.activeTab = tab;
    this.clearMessages();
  }

  // ==================== MODAL OPERATIONS ====================

  openCreateModal(type: typeof this.modalType): void {
    this.modalMode = 'create';
    this.modalType = type;
    this.formData = this.getEmptyFormData(type);
    this.showModal = true;
  }

  openEditModal(type: typeof this.modalType, item: any): void {
    this.modalMode = 'edit';
    this.modalType = type;
    this.formData = { ...item };
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.formData = {};
  }

  private getEmptyFormData(type: typeof this.modalType): any {
    switch (type) {
      case 'role':
        return { name: '', description: '', permissions: '[]' };
      case 'stage':
        return { name: '', description: '', order: this.stages.length + 1 };
      case 'artifactType':
        return { name: '', description: '', defaultMandatory: false, applicableStages: '[]' };
      case 'customField':
        return { name: '', fieldType: CustomFieldType.Text, options: '', isRequired: false, artifactTypeId: null };
      case 'version':
        return { createdBy: '', description: '' };
      default:
        return {};
    }
  }

  saveItem(): void {
    this.clearMessages();

    if (this.modalType === 'version') {
      this.saveVersion();
      return;
    }

    if (this.modalMode === 'create') {
      this.createItem();
    } else {
      this.updateItem();
    }
  }

  private createItem(): void {
    switch (this.modalType) {
      case 'role':
        this.configService.createRole(this.formData).subscribe({
          next: () => {
            this.showSuccess('Rol creado exitosamente');
            this.loadRoles();
            this.closeModal();
          },
          error: (err) => this.showError(err.error?.message || 'Error al crear rol')
        });
        break;
      case 'stage':
        this.configService.createStage(this.formData).subscribe({
          next: () => {
            this.showSuccess('Etapa creada exitosamente');
            this.loadStages();
            this.closeModal();
          },
          error: (err) => this.showError(err.error?.message || 'Error al crear etapa')
        });
        break;
      case 'artifactType':
        this.configService.createArtifactType(this.formData).subscribe({
          next: () => {
            this.showSuccess('Tipo de artefacto creado exitosamente');
            this.loadArtifactTypes();
            this.closeModal();
          },
          error: (err) => this.showError(err.error?.message || 'Error al crear tipo de artefacto')
        });
        break;
      case 'customField':
        this.configService.createCustomField(this.formData).subscribe({
          next: () => {
            this.showSuccess('Campo personalizado creado exitosamente');
            this.loadCustomFields();
            this.closeModal();
          },
          error: (err) => this.showError(err.error?.message || 'Error al crear campo personalizado')
        });
        break;
    }
  }

  private updateItem(): void {
    const id = this.formData.id;
    switch (this.modalType) {
      case 'role':
        this.configService.updateRole(id, this.formData).subscribe({
          next: () => {
            this.showSuccess('Rol actualizado exitosamente');
            this.loadRoles();
            this.closeModal();
          },
          error: (err) => this.showError(err.error?.message || 'Error al actualizar rol')
        });
        break;
      case 'stage':
        this.configService.updateStage(id, this.formData).subscribe({
          next: () => {
            this.showSuccess('Etapa actualizada exitosamente');
            this.loadStages();
            this.closeModal();
          },
          error: (err) => this.showError(err.error?.message || 'Error al actualizar etapa')
        });
        break;
      case 'artifactType':
        this.configService.updateArtifactType(id, this.formData).subscribe({
          next: () => {
            this.showSuccess('Tipo de artefacto actualizado exitosamente');
            this.loadArtifactTypes();
            this.closeModal();
          },
          error: (err) => this.showError(err.error?.message || 'Error al actualizar tipo de artefacto')
        });
        break;
      case 'customField':
        this.configService.updateCustomField(id, this.formData).subscribe({
          next: () => {
            this.showSuccess('Campo personalizado actualizado exitosamente');
            this.loadCustomFields();
            this.closeModal();
          },
          error: (err) => this.showError(err.error?.message || 'Error al actualizar campo personalizado')
        });
        break;
    }
  }

  deleteItem(type: typeof this.modalType, id: number): void {
    if (!confirm('¿Está seguro de eliminar este elemento?')) return;

    switch (type) {
      case 'role':
        this.configService.deleteRole(id).subscribe({
          next: () => {
            this.showSuccess('Rol eliminado');
            this.loadRoles();
          },
          error: (err) => this.showError(err.error?.message || 'Error al eliminar')
        });
        break;
      case 'stage':
        this.configService.deleteStage(id).subscribe({
          next: () => {
            this.showSuccess('Etapa eliminada');
            this.loadStages();
          },
          error: (err) => this.showError(err.error?.message || 'Error al eliminar')
        });
        break;
      case 'artifactType':
        this.configService.deleteArtifactType(id).subscribe({
          next: () => {
            this.showSuccess('Tipo de artefacto eliminado');
            this.loadArtifactTypes();
          },
          error: (err) => this.showError(err.error?.message || 'Error al eliminar')
        });
        break;
      case 'customField':
        this.configService.deleteCustomField(id).subscribe({
          next: () => {
            this.showSuccess('Campo personalizado eliminado');
            this.loadCustomFields();
          },
          error: (err) => this.showError(err.error?.message || 'Error al eliminar')
        });
        break;
    }
  }

  // ==================== VERSION OPERATIONS ====================

  private saveVersion(): void {
    this.configService.saveVersion(this.formData).subscribe({
      next: () => {
        this.showSuccess('Versión de configuración guardada');
        this.loadVersions();
        this.closeModal();
      },
      error: (err) => this.showError(err.error?.message || 'Error al guardar versión')
    });
  }

  restoreVersion(versionId: number): void {
    if (!confirm('¿Está seguro de restaurar esta versión? Se sobrescribirá la configuración actual.')) return;

    this.configService.restoreVersion(versionId).subscribe({
      next: (result) => {
        this.showSuccess(`Versión ${result.restoredVersion} restaurada exitosamente`);
        this.loadAllData();
      },
      error: (err) => this.showError(err.error?.message || 'Error al restaurar versión')
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

  private clearMessages(): void {
    this.error = null;
    this.successMessage = null;
  }

  getModalTitle(): string {
    const action = this.modalMode === 'create' ? 'Crear' : 'Editar';
    const types: Record<typeof this.modalType, string> = {
      role: 'Rol',
      stage: 'Etapa',
      artifactType: 'Tipo de Artefacto',
      customField: 'Campo Personalizado',
      version: 'Guardar Versión'
    };
    return this.modalType === 'version' ? types.version : `${action} ${types[this.modalType]}`;
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
