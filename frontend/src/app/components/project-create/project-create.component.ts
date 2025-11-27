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
    tags: '',
    objetivos: '',
    alcance: '',
    cronogramaInicial: '',
    responsables: '',
    hitos: ''
  };

  loading = false;
  error: string | null = null;

  // Structured helpers for better UX
  cronogramaItems: { date: string; description: string }[] = [
    { date: this.getTodayDate(), description: '' }
  ];

  responsablesList: string[] = [''];

  hitosList: string[] = [''];

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
    // Serialize structured fields to JSON strings to store in backend text columns
    const cronogramaString = JSON.stringify(this.cronogramaItems.filter(ci => ci.description?.trim()));
    const responsablesString = JSON.stringify(this.responsablesList.map(r => r.trim()).filter(r => r));
    const hitosString = JSON.stringify(this.hitosList.map(h => h.trim()).filter(h => h));

    const request: CreateProjectRequest = {
      ...this.formData,
      description: this.formData.description?.trim() || undefined,
      responsiblePerson: this.formData.responsiblePerson?.trim() || undefined,
      tags: this.formData.tags?.trim() || undefined,
      objetivos: this.formData.objetivos?.trim() || undefined,
      alcance: this.formData.alcance?.trim() || undefined,
      cronogramaInicial: cronogramaString || undefined,
      responsables: responsablesString || undefined,
      hitos: hitosString || undefined
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

  // Cronograma helpers
  addCronogramaItem(): void {
    this.cronogramaItems.push({ date: this.getTodayDate(), description: '' });
  }

  removeCronogramaItem(index: number): void {
    if (this.cronogramaItems.length > 1) {
      this.cronogramaItems.splice(index, 1);
    }
  }

  // Responsables helpers
  addResponsable(): void {
    this.responsablesList.push('');
  }

  removeResponsable(index: number): void {
    if (this.responsablesList.length > 1) {
      this.responsablesList.splice(index, 1);
    }
  }

  // Hitos helpers
  addHito(): void {
    this.hitosList.push('');
  }

  removeHito(index: number): void {
    if (this.hitosList.length > 1) {
      this.hitosList.splice(index, 1);
    }
  }

  moveHitoUp(index: number): void {
    if (index <= 0) return;
    const tmp = this.hitosList[index - 1];
    this.hitosList[index - 1] = this.hitosList[index];
    this.hitosList[index] = tmp;
  }

  moveHitoDown(index: number): void {
    if (index >= this.hitosList.length - 1) return;
    const tmp = this.hitosList[index + 1];
    this.hitosList[index + 1] = this.hitosList[index];
    this.hitosList[index] = tmp;
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
