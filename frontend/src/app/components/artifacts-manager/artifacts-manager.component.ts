import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms'; 
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Artifact, ArtifactType } from '../../models/artifact.model';

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
  artifactForm!: FormGroup;
  selectedFile: File | null = null;

  private apiUrl = 'http://localhost:5062/api/artifacts';

  artifactTypes = Object.keys(ArtifactType)
    .filter(key => !isNaN(Number(ArtifactType[key as keyof typeof ArtifactType])))
    .map(key => ({ key: key, value: ArtifactType[key as keyof typeof ArtifactType] }));

  constructor(
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
      isMandatory: [false]
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
}