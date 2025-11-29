import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Artifact, ArtifactType } from '../../models/artifact.model';
import { ArtifactService } from '../../services/artifactService';

@Component({
  selector: 'app-artifacts-manager',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './artifacts-manager.component.html',
  styleUrls: ['./artifacts-manager.component.scss']
})
export class ArtifactsManagerComponent implements OnChanges {
  @Input() phaseId!: number;

  artifacts: Artifact[] = [];
  artifactForm: FormGroup;
  selectedFile: File | null = null;

  artifactTypes = Object.keys(ArtifactType)
    .filter(key => !isNaN(Number(ArtifactType[key as keyof typeof ArtifactType])))
    .map(key => ({ key: key, value: ArtifactType[key as keyof typeof ArtifactType] }));

  constructor(
    private artifactService: ArtifactService,
    private fb: FormBuilder
  ) {
    this.artifactForm = this.fb.group({
      type: [null, Validators.required],
      author: ['', Validators.required],
      isMandatory: [false],
      content: [''],
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['phaseId'] && this.phaseId) {
      this.loadArtifacts();
    }
  }

  loadArtifacts(): void {
    this.artifactService.getArtifactsForPhase(this.phaseId).subscribe((data: Artifact[]) => {
      this.artifacts = data;
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
    formData.append('projectPhaseId', this.phaseId.toString());
    formData.append('type', this.artifactForm.get('type')?.value);
    formData.append('author', this.artifactForm.get('author')?.value);
    formData.append('isMandatory', this.artifactForm.get('isMandatory')?.value);
    formData.append('content', this.artifactForm.get('content')?.value);

    if (this.selectedFile) {
      formData.append('file', this.selectedFile, this.selectedFile.name);
    }

    this.artifactService.createArtifact(formData).subscribe((newArtifact: Artifact) => {
      this.loadArtifacts();
      this.artifactForm.reset();
      this.selectedFile = null;
      const fileInput = document.getElementById('fileInput') as HTMLInputElement;
      if (fileInput) {
        fileInput.value = '';
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