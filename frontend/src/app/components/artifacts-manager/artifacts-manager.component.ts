import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms'; 
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Artifact, ArtifactType } from '../../models/artifact.model';
import { ArtifactService } from '../../services/artifactService';

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
export class ArtifactsManagerComponent implements OnChanges {
  @Input() phaseId!: number;

  artifacts: Artifact[] = [];
  workflows: any[] = []; // AGREGAR ESTA LÍNEA
  artifactForm!: FormGroup;
  selectedFile: File | null = null;

  private apiUrl = 'http://localhost:5062/api/artifacts';
  private workflowApiUrl = 'http://localhost:5062/api/workflows'; // AGREGAR ESTA LÍNEA

  artifactTypes = Object.keys(ArtifactType)
    .filter(key => !isNaN(Number(ArtifactType[key as keyof typeof ArtifactType])))
    .map(key => ({ key: key, value: ArtifactType[key as keyof typeof ArtifactType] }));

  constructor(
    private artifactService: ArtifactService,
    private fb: FormBuilder,
    private http: HttpClient
  ) {
    this.initForm();
  }

  initForm() {
    this.artifactForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      type: ['', Validators.required],
      author: [''],
      content: [''],
      isMandatory: [false],
      workflowId: [null] // AGREGAR ESTA LÍNEA
    });
  }

  ngOnInit() {
    this.loadWorkflows(); // AGREGAR ESTA LÍNEA
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['phaseId'] && this.phaseId) {
      this.loadArtifacts();
    }
  }

  loadWorkflows() { // AGREGAR ESTE MÉTODO COMPLETO
    this.http.get<any[]>(this.workflowApiUrl).subscribe({
      next: (data) => {
        this.workflows = data;
      },
      error: (err) => console.error('Error loading workflows:', err)
    });
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

  onSubmit(): void {
    if (this.artifactForm.invalid) {
      return;
    }

    const formData = new FormData();
    formData.append('name', this.artifactForm.value.name);
    formData.append('description', this.artifactForm.value.description || '');
    formData.append('type', this.artifactForm.value.type);
    formData.append('author', this.artifactForm.value.author || '');
    formData.append('content', this.artifactForm.value.content || '');
    formData.append('isMandatory', this.artifactForm.value.isMandatory.toString());
    formData.append('projectPhaseId', this.phaseId.toString());
    
    // AGREGAR ESTAS LÍNEAS
    if (this.artifactForm.value.workflowId) {
      formData.append('workflowId', this.artifactForm.value.workflowId);
    }

    if (this.selectedFile) {
      formData.append('file', this.selectedFile);
    }

    this.http.post(this.apiUrl, formData).subscribe({
      next: () => {
        this.loadArtifacts();
        this.artifactForm.reset({ isMandatory: false, workflowId: null });
        this.selectedFile = null;
        alert('Artefacto creado exitosamente');
      },
      error: (err) => {
        console.error('Error al crear artefacto:', err);
        alert('Error al crear el artefacto');
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

  // AGREGAR ESTE MÉTODO
  onStatusChange(artifact: Artifact, event: Event): void {
    const select = event.target as HTMLSelectElement;
    const newStepId = Number(select.value);

    this.artifactService.updateStatus(artifact.id, newStepId).subscribe({
      next: () => {
        console.log('Estado actualizado');
        // Opcional: recargar artefactos para asegurar sincronización
        // this.loadArtifacts(); 
        
        // Actualizar localmente para feedback inmediato
        artifact.currentStepId = newStepId;
        const step = artifact.workflow?.steps.find(s => s.id === newStepId);
        if(step) artifact.currentStep = step;
      },
      error: (err) => console.error('Error al cambiar estado', err)
    });
  }
}