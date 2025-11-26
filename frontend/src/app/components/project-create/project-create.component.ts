import { Component } from '@angular/core';

import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterModule } from '@angular/router';
import { ProjectService } from '../../services/project.service';
import { CreateProjectRequest } from '../../models/project.model';

@Component({
  selector: 'app-project-create',
  standalone: true,
  imports: [FormsModule, RouterModule, RouterLink],
  templateUrl: './project-create.component.html',
  styleUrls: ['./project-create.component.scss']
})
export class ProjectCreateComponent {
  formData: CreateProjectRequest = {
    name: '',
    code: '',
    startDate: this.getTodayDate(),
    description: '',
    responsiblePerson: '',
    tags: ''
  };

  loading = false;
  error: string | null = null;

  constructor(
    private projectService: ProjectService,
    private router: Router
  ) {}

  private getTodayDate(): string {
    const today = new Date();
    return today.toISOString().split('T')[0];
  }

  onSubmit(): void {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    this.error = null;

    // Limpiar campos opcionales vacíos
    const request: CreateProjectRequest = {
      ...this.formData,
      description: this.formData.description?.trim() || undefined,
      responsiblePerson: this.formData.responsiblePerson?.trim() || undefined,
      tags: this.formData.tags?.trim() || undefined
    };

    this.projectService.createProject(request).subscribe({
      next: (project) => {
        this.router.navigate(['/projects', project.id]);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Error al crear el proyecto';
        console.error('Error:', err);
      }
    });
  }

  private validateForm(): boolean {
    if (!this.formData.name.trim()) {
      this.error = 'El nombre es requerido';
      return false;
    }

    if (!this.formData.code.trim()) {
      this.error = 'El código es requerido';
      return false;
    }

    if (!this.formData.startDate) {
      this.error = 'La fecha de inicio es requerida';
      return false;
    }

    return true;
  }

  onCancel(): void {
    this.router.navigate(['/projects']);
  }
}
