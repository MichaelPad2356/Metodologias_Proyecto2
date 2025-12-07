import { Component, Input, OnChanges, SimpleChanges, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Artifact, ArtifactType, Workflow } from '../../models/artifact.model';
import { ArtifactService } from '../../services/artifactService';
import { PermissionService } from '../../services/permission.service';

@Component({
  selector: 'app-artifacts-manager',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule 
  ],
  templateUrl: './artifacts-manager.component.html',
  styleUrls: ['./artifacts-manager.component.scss']
})
export class ArtifactsManagerComponent implements OnChanges, OnInit {
  @Input() phaseId!: number;

  artifacts: Artifact[] = [];
  workflows: Workflow[] = [];
  artifactForm!: FormGroup;
  versionForm!: FormGroup;
  selectedFile: File | null = null;
  selectedVersionFile: File | null = null;
  selectedArtifactIdForVersion: number | null = null;

  canCreate: boolean = false;
  canApprove: boolean = false;
  canReview: boolean = false;
  canEdit: boolean = false;
  canSetRequired: boolean = false;
  isReadOnly: boolean = true;

  private apiUrl = '/api/artifacts';

  artifactTypes = Object.keys(ArtifactType)
    .filter(key => !isNaN(Number(ArtifactType[key as keyof typeof ArtifactType])))
    .map(key => ({ key: key, value: ArtifactType[key as keyof typeof ArtifactType] }));

  constructor(
    private artifactService: ArtifactService,
    private fb: FormBuilder,
    private permService: PermissionService,
    private http: HttpClient
  ) {
    this.initForm();
  }

  initForm() {
    this.artifactForm = this.fb.group({
      type: ['', Validators.required],
      author: ['', Validators.required],
      content: [''],
      repositoryUrl: [''],
      assignedTo: [''],
      isMandatory: [false],
      workflowId: [null],
      buildIdentifier: [''],
      buildDownloadUrl: ['']
    });

    this.versionForm = this.fb.group({
      author: ['', Validators.required],
      content: [''],
      repositoryUrl: ['']
    });
  }

  ngOnInit(): void {
    this.permService.role$.subscribe(() => {
      this.canCreate = this.permService.canCreateArtifact();
      this.canEdit = this.permService.canEditArtifact();
      this.canApprove = this.permService.canApproveArtifact();
      this.canReview = this.permService.canReviewArtifact();
      this.canSetRequired = this.permService.canSetArtifactRequired();
      this.isReadOnly = this.permService.isReadOnlyForArtifacts();
    });
    this.loadWorkflows();
  }

  loadWorkflows(): void {
    this.artifactService.getWorkflows().subscribe({
      next: (data) => {
        this.workflows = data;
      },
      error: (err) => console.error('Error loading workflows:', err)
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['phaseId'] && this.phaseId) {
      this.loadArtifacts();
    }
  }

  loadArtifacts(): void {
    this.http.get<Artifact[]>(`${this.apiUrl}/phase/${this.phaseId}`).subscribe({
      next: (data) => {
        this.artifacts = data;
      },
      error: (err) => console.error('Error al cargar artefactos:', err)
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }

  onVersionFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedVersionFile = input.files[0];
    }
  }

  openVersionForm(artifactId: number): void {
    this.selectedArtifactIdForVersion = artifactId;
    this.versionForm.reset();
    this.selectedVersionFile = null;
  }

  cancelVersionForm(): void {
    this.selectedArtifactIdForVersion = null;
    this.versionForm.reset();
    this.selectedVersionFile = null;
  }

  onSubmitVersion(): void {
    if (this.versionForm.invalid || !this.selectedArtifactIdForVersion) {
      return;
    }

    const formData = new FormData();
    formData.append('author', this.versionForm.get('author')?.value);
    formData.append('content', this.versionForm.get('content')?.value || '');
    formData.append('repositoryUrl', this.versionForm.get('repositoryUrl')?.value || '');

    if (this.selectedVersionFile) {
      formData.append('file', this.selectedVersionFile, this.selectedVersionFile.name);
    }

    this.artifactService.addVersion(this.selectedArtifactIdForVersion, formData).subscribe({
      next: () => {
        this.loadArtifacts();
        this.cancelVersionForm();
      },
      error: (err) => {
        console.error('Error adding version:', err);
        alert('Error al agregar nueva versión');
      }
    });
  }

  onSubmit(): void {
    if (this.artifactForm.invalid) {
      return;
    }

    const formData = new FormData();
    formData.append('projectPhaseId', this.phaseId.toString());
    formData.append('type', this.artifactForm.get('type')?.value);
    formData.append('author', this.artifactForm.get('author')?.value);
    formData.append('isMandatory', this.artifactForm.get('isMandatory')?.value.toString());
    formData.append('content', this.artifactForm.get('content')?.value || '');
    formData.append('repositoryUrl', this.artifactForm.get('repositoryUrl')?.value || '');
    formData.append('assignedTo', this.artifactForm.get('assignedTo')?.value || '');
    formData.append('buildIdentifier', this.artifactForm.get('buildIdentifier')?.value || '');
    formData.append('buildDownloadUrl', this.artifactForm.get('buildDownloadUrl')?.value || '');
    
    const workflowId = this.artifactForm.get('workflowId')?.value;
    if (workflowId) {
      formData.append('workflowId', workflowId);
    }

    if (this.selectedFile) {
      formData.append('file', this.selectedFile);
    }

    this.http.post(this.apiUrl, formData).subscribe({
      next: () => {
        this.loadArtifacts();
        this.artifactForm.reset({ isMandatory: false });
        this.selectedFile = null;
        alert('Artefacto creado exitosamente');
      },
      error: (err: any) => {
        console.error('Error al crear artefacto:', err);
        alert('Error al crear el artefacto');
      }
    });
  }

  advanceWorkflowStep(artifact: Artifact): void {
    if (!artifact.workflowId || !artifact.currentStepId) return;

    const workflow = this.workflows.find(w => w.id === artifact.workflowId);
    if (!workflow) return;

    const currentStepIndex = workflow.steps.findIndex(s => s.id === artifact.currentStepId);
    if (currentStepIndex === -1 || currentStepIndex >= workflow.steps.length - 1) return;

    const nextStep = workflow.steps[currentStepIndex + 1];

    if (!confirm(`¿Avanzar al paso "${nextStep.name}"?`)) return;

    this.artifactService.updateWorkflowStep(artifact.id, nextStep.id).subscribe({
      next: () => {
        this.loadArtifacts();
      },
      error: (err) => {
        console.error('Error advancing workflow step:', err);
        alert('Error al avanzar el paso del flujo de trabajo');
      }
    });
  }

  getStatusClass(status: any): string {
    switch (status) {
      case 0: // Pending
        return 'status-pending';
      case 1: // InReview
        return 'status-inreview';
      case 2: // Approved
        return 'status-approved';
      default:
        return 'status-pending';
    }
  }

  updateArtifactStatus(artifact: Artifact, newStatus: string): void {
    if (!confirm(`¿Cambiar estado a ${newStatus}?`)) return;

    this.artifactService.updateArtifactStatus(artifact.id, newStatus).subscribe({
      next: () => {
        this.loadArtifacts();
      },
      error: (err) => {
        console.error('Error actualizando artefacto:', err);
        alert('Error al actualizar estado');
      }
    });
  }
}