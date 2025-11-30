import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { 
  Artifact, 
  ArtifactType, 
  ArtifactStatus, 
  ArtifactTypeLabels,
  ClosureChecklistItem,
  DefaultClosureChecklist,
  ArtifactVersion,
  VersionComparison
} from '../../models/artifact.model';
import { ArtifactService, TransitionArtifactsResponse, ClosureValidationResponse } from '../../services/artifactService';

@Component({
  selector: 'app-transition-artifacts',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './transition-artifacts.component.html',
  styleUrl: './transition-artifacts.component.scss'
})
export class TransitionArtifactsComponent implements OnInit, OnChanges {
  @Input() projectId!: number;

  artifacts: Artifact[] = [];
  phaseId: number | null = null;
  loading = false;
  error = '';
  
  // Tipos de artefactos de transición
  transitionTypes = [
    { type: ArtifactType.UserManual, label: 'Manual de Usuario', icon: '📖' },
    { type: ArtifactType.TechnicalManual, label: 'Manual Técnico', icon: '🔧' },
    { type: ArtifactType.DeploymentPlan, label: 'Plan de Despliegue', icon: '🚀' },
    { type: ArtifactType.ClosureDocument, label: 'Documento de Cierre', icon: '✅' },
    { type: ArtifactType.FinalBuild, label: 'Build Final', icon: '📦' },
    { type: ArtifactType.BetaTestReport, label: 'Reporte de Pruebas Beta', icon: '🧪' },
  ];

  missingMandatory: { type: number; typeName: string }[] = [];
  canClose = false;
  
  // Modales
  showArtifactModal = false;
  showVersionModal = false;
  showChecklistModal = false;
  showBuildModal = false;
  showHistoryModal = false;  // HU-010: Modal de historial
  showCompareModal = false;  // HU-010: Modal de comparación
  
  // Formularios
  selectedArtifactType: ArtifactType | null = null;
  selectedArtifact: Artifact | null = null;
  
  newArtifactForm = {
    author: '',
    content: '',
    observations: '',  // HU-010: Observaciones iniciales
    file: null as File | null,
    buildIdentifier: '',
    buildDownloadUrl: ''
  };

  newVersionForm = {
    author: '',
    content: '',
    observations: '',  // HU-010: Descripción de cambios obligatoria
    file: null as File | null
  };

  // HU-010: Control de versiones
  versionHistory: ArtifactVersion[] = [];
  selectedVersions: { v1: number | null; v2: number | null } = { v1: null, v2: null };
  versionComparison: VersionComparison | null = null;

  // Checklist de cierre
  closureChecklist: ClosureChecklistItem[] = [];
  
  // Validación de cierre
  closureValidation: ClosureValidationResponse | null = null;
  showValidationModal = false;

  constructor(
    private artifactService: ArtifactService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Obtener projectId desde la ruta si no viene como Input
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.projectId = +params['id'];
        this.loadTransitionArtifacts();
      } else if (this.projectId) {
        this.loadTransitionArtifacts();
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['projectId'] && this.projectId) {
      this.loadTransitionArtifacts();
    }
  }

  loadTransitionArtifacts(): void {
    this.loading = true;
    this.error = '';
    
    this.artifactService.getTransitionArtifacts(this.projectId).subscribe({
      next: (data: TransitionArtifactsResponse) => {
        this.phaseId = data.phaseId;
        this.artifacts = data.artifacts;
        this.missingMandatory = data.missingMandatory;
        this.canClose = data.canClose;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar los artefactos de transición';
        console.error('Error:', err);
        this.loading = false;
      }
    });
  }

  getArtifactByType(type: ArtifactType): Artifact | undefined {
    return this.artifacts.find(a => a.type === type);
  }

  getTypeLabel(type: ArtifactType): string {
    return ArtifactTypeLabels[type] || type.toString();
  }

  getStatusLabel(status: ArtifactStatus): string {
    switch (status) {
      case ArtifactStatus.Pending: return 'Pendiente';
      case ArtifactStatus.InReview: return 'En Revisión';
      case ArtifactStatus.Approved: return 'Aprobado';
      default: return 'Desconocido';
    }
  }

  getStatusClass(status: ArtifactStatus): string {
    switch (status) {
      case ArtifactStatus.Pending: return 'status-pending';
      case ArtifactStatus.InReview: return 'status-review';
      case ArtifactStatus.Approved: return 'status-approved';
      default: return '';
    }
  }

  // Abrir modal para crear artefacto
  openCreateModal(type: ArtifactType): void {
    this.selectedArtifactType = type;
    this.resetArtifactForm();
    
    // Si es documento de cierre, inicializar checklist
    if (type === ArtifactType.ClosureDocument) {
      this.closureChecklist = JSON.parse(JSON.stringify(DefaultClosureChecklist));
    }
    
    this.showArtifactModal = true;
  }

  // Abrir modal para agregar versión
  openVersionModal(artifact: Artifact): void {
    this.selectedArtifact = artifact;
    this.resetVersionForm();
    this.showVersionModal = true;
  }

  // Abrir modal de checklist para documento de cierre existente
  openChecklistModal(artifact: Artifact): void {
    this.selectedArtifact = artifact;
    if (artifact.closureChecklist) {
      this.closureChecklist = JSON.parse(JSON.stringify(artifact.closureChecklist));
    } else if (artifact.closureChecklistJson) {
      try {
        this.closureChecklist = JSON.parse(artifact.closureChecklistJson);
      } catch {
        this.closureChecklist = JSON.parse(JSON.stringify(DefaultClosureChecklist));
      }
    } else {
      this.closureChecklist = JSON.parse(JSON.stringify(DefaultClosureChecklist));
    }
    this.showChecklistModal = true;
  }

  // Abrir modal para editar Build Final
  openBuildModal(artifact: Artifact): void {
    this.selectedArtifact = artifact;
    this.newArtifactForm.buildIdentifier = artifact.buildIdentifier || '';
    this.newArtifactForm.buildDownloadUrl = artifact.buildDownloadUrl || '';
    this.showBuildModal = true;
  }

  onFileSelected(event: Event, form: 'artifact' | 'version'): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      if (form === 'artifact') {
        this.newArtifactForm.file = input.files[0];
      } else {
        this.newVersionForm.file = input.files[0];
      }
    }
  }

  // Crear nuevo artefacto
  createArtifact(): void {
    if (!this.phaseId || this.selectedArtifactType === null) return;

    const formData = new FormData();
    formData.append('projectPhaseId', this.phaseId.toString());
    formData.append('type', this.selectedArtifactType.toString());
    formData.append('author', this.newArtifactForm.author);
    formData.append('isMandatory', 'true');
    formData.append('content', this.newArtifactForm.content);
    formData.append('observations', this.newArtifactForm.observations || 'Versión inicial');  // HU-010

    if (this.newArtifactForm.file) {
      formData.append('file', this.newArtifactForm.file);
    }

    // Campos específicos para Build Final
    if (this.selectedArtifactType === ArtifactType.FinalBuild) {
      formData.append('buildIdentifier', this.newArtifactForm.buildIdentifier);
      formData.append('buildDownloadUrl', this.newArtifactForm.buildDownloadUrl);
    }

    // Campos para Documento de Cierre
    if (this.selectedArtifactType === ArtifactType.ClosureDocument) {
      formData.append('closureChecklistJson', JSON.stringify(this.closureChecklist));
    }

    this.artifactService.createArtifact(formData).subscribe({
      next: () => {
        this.closeModals();
        this.loadTransitionArtifacts();
        alert('✅ Artefacto creado correctamente');
      },
      error: (err) => {
        console.error('Error al crear artefacto:', err);
        alert('❌ Error al crear el artefacto');
      }
    });
  }

  // Agregar nueva versión
  addVersion(): void {
    if (!this.selectedArtifact) return;

    // HU-010: Validar que observaciones no esté vacío
    if (!this.newVersionForm.observations.trim()) {
      alert('⚠️ La descripción de cambios es obligatoria');
      return;
    }

    const formData = new FormData();
    formData.append('author', this.newVersionForm.author);
    formData.append('content', this.newVersionForm.content);
    formData.append('observations', this.newVersionForm.observations);  // HU-010

    if (this.newVersionForm.file) {
      formData.append('file', this.newVersionForm.file);
    }

    this.artifactService.addVersion(this.selectedArtifact.id, formData).subscribe({
      next: () => {
        this.closeModals();
        this.loadTransitionArtifacts();
        alert('✅ Nueva versión agregada correctamente');
      },
      error: (err: Error) => {
        console.error('Error al agregar versión:', err);
        alert('❌ Error al agregar la versión');
      }
    });
  }

  // Guardar checklist de cierre
  saveChecklist(): void {
    if (!this.selectedArtifact) return;

    this.artifactService.updateArtifact(this.selectedArtifact.id, {
      closureChecklistJson: JSON.stringify(this.closureChecklist)
    }).subscribe({
      next: () => {
        this.closeModals();
        this.loadTransitionArtifacts();
        alert('✅ Checklist actualizado correctamente');
      },
      error: (err) => {
        console.error('Error al guardar checklist:', err);
        alert('❌ Error al guardar el checklist');
      }
    });
  }

  // Guardar información del Build
  saveBuildInfo(): void {
    if (!this.selectedArtifact) return;

    this.artifactService.updateArtifact(this.selectedArtifact.id, {
      buildIdentifier: this.newArtifactForm.buildIdentifier,
      buildDownloadUrl: this.newArtifactForm.buildDownloadUrl
    }).subscribe({
      next: () => {
        this.closeModals();
        this.loadTransitionArtifacts();
        alert('✅ Información del Build actualizada');
      },
      error: (err) => {
        console.error('Error al guardar Build:', err);
        alert('❌ Error al guardar la información del Build');
      }
    });
  }

  // Cambiar estado del artefacto
  changeStatus(artifact: Artifact, status: ArtifactStatus): void {
    this.artifactService.updateArtifact(artifact.id, { status }).subscribe({
      next: () => {
        this.loadTransitionArtifacts();
        alert('✅ Estado actualizado');
      },
      error: (err) => {
        console.error('Error al cambiar estado:', err);
        alert('❌ Error al cambiar el estado');
      }
    });
  }

  // Validar cierre del proyecto
  validateClosure(): void {
    this.artifactService.validateProjectClosure(this.projectId).subscribe({
      next: (validation) => {
        this.closureValidation = validation;
        this.showValidationModal = true;
      },
      error: (err) => {
        console.error('Error al validar cierre:', err);
        alert('❌ Error al validar el cierre del proyecto');
      }
    });
  }

  // Marcar item de checklist como completado
  toggleChecklistItem(item: ClosureChecklistItem): void {
    item.isCompleted = !item.isCompleted;
    if (item.isCompleted) {
      item.completedDate = new Date().toISOString();
    } else {
      item.completedDate = undefined;
      item.completedBy = undefined;
    }
  }

  // Calcular progreso del checklist
  getChecklistProgress(): number {
    if (this.closureChecklist.length === 0) return 0;
    const completed = this.closureChecklist.filter(c => c.isCompleted).length;
    return Math.round((completed / this.closureChecklist.length) * 100);
  }

  getMandatoryChecklistProgress(): number {
    const mandatory = this.closureChecklist.filter(c => c.isMandatory);
    if (mandatory.length === 0) return 0;
    const completed = mandatory.filter(c => c.isCompleted).length;
    return Math.round((completed / mandatory.length) * 100);
  }

  closeModals(): void {
    this.showArtifactModal = false;
    this.showVersionModal = false;
    this.showChecklistModal = false;
    this.showBuildModal = false;
    this.showValidationModal = false;
    this.showHistoryModal = false;  // HU-010
    this.showCompareModal = false;  // HU-010
    this.selectedArtifact = null;
    this.selectedArtifactType = null;
    this.versionComparison = null;  // HU-010
  }

  resetArtifactForm(): void {
    this.newArtifactForm = {
      author: '',
      content: '',
      observations: '',
      file: null,
      buildIdentifier: '',
      buildDownloadUrl: ''
    };
  }

  resetVersionForm(): void {
    this.newVersionForm = {
      author: '',
      content: '',
      observations: '',
      file: null
    };
  }

  // HU-010: Abrir modal de historial de versiones
  openHistoryModal(artifact: Artifact): void {
    this.selectedArtifact = artifact;
    this.versionHistory = artifact.versions || [];
    this.selectedVersions = { v1: null, v2: null };
    this.versionComparison = null;
    this.showHistoryModal = true;
  }

  // HU-010: Comparar versiones seleccionadas
  compareVersions(): void {
    if (!this.selectedArtifact || !this.selectedVersions.v1 || !this.selectedVersions.v2) {
      alert('Por favor selecciona dos versiones para comparar');
      return;
    }

    this.artifactService.compareVersions(
      this.selectedArtifact.id,
      this.selectedVersions.v1,
      this.selectedVersions.v2
    ).subscribe({
      next: (comparison: VersionComparison) => {
        this.versionComparison = comparison;
        this.showCompareModal = true;
      },
      error: (err: Error) => {
        console.error('Error al comparar versiones:', err);
        alert('❌ Error al comparar versiones');
      }
    });
  }

  // HU-010: Exportar historial de versiones
  exportHistory(): void {
    if (!this.selectedArtifact) return;

    this.artifactService.exportVersionHistory(this.selectedArtifact.id).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `historial_${this.selectedArtifact!.typeName}_${new Date().toISOString().split('T')[0]}.json`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err: Error) => {
        console.error('Error al exportar historial:', err);
        alert('❌ Error al exportar el historial');
      }
    });
  }

  // HU-010: Descargar versión específica
  downloadVersion(versionNumber: number): void {
    if (!this.selectedArtifact) return;

    this.artifactService.downloadVersion(this.selectedArtifact.id, versionNumber).subscribe({
      next: (blob: Blob) => {
        const version = this.versionHistory.find(v => v.versionNumber === versionNumber);
        const fileName = version?.originalFileName || `version_${versionNumber}`;
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err: Error) => {
        console.error('Error al descargar versión:', err);
        alert('❌ Error al descargar la versión');
      }
    });
  }

  // HU-010: Formatear tamaño de archivo
  formatFileSize(bytes?: number): string {
    if (!bytes) return 'N/A';
    const units = ['B', 'KB', 'MB', 'GB'];
    let unitIndex = 0;
    let size = bytes;
    while (size >= 1024 && unitIndex < units.length - 1) {
      size /= 1024;
      unitIndex++;
    }
    return `${size.toFixed(1)} ${units[unitIndex]}`;
  }

  // HU-010: Formatear fecha
  formatDate(date: Date | string): string {
    return new Date(date).toLocaleString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // Helpers para enum
  ArtifactType = ArtifactType;
  ArtifactStatus = ArtifactStatus;
}
