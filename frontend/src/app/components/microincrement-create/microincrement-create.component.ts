import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MicroincrementService } from '../../services/microincrement.service';
import { ProjectService } from '../../services/project.service';
import { CreateMicroincrementRequest } from '../../models/microincrement.model';
import { MicroincrementListComponent } from '../microincrement-list/microincrement-list.component';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-microincrement-create',
  standalone: true,
  imports: [CommonModule, FormsModule, MicroincrementListComponent],
  templateUrl: './microincrement-create.component.html',
  styleUrls: ['./microincrement-create.component.scss']
})
export class MicroincrementCreateComponent implements OnInit {
  form: CreateMicroincrementRequest = {
    title: '',
    description: '',
    projectPhaseId: 0,
    deliverableId: undefined,
    author: ''
  };

  projects: any[] = [];
  phases: any[] = [];
  deliverables: any[] = [];
  selectedProjectId: number | null = null;
  
  loading = false;
  loadingData = true;
  error = '';
  success = false;

  constructor(
    private microincrementService: MicroincrementService,
    private projectService: ProjectService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.projectService.getAllProjects().subscribe({
      next: (projects) => {
        this.projects = projects;
        this.loadingData = false;
      },
      error: (err) => {
        console.error('Error loading projects:', err);
        this.error = 'Error al cargar proyectos';
        this.loadingData = false;
      }
    });
  }

  onProjectChange(projectId: number): void {
    this.selectedProjectId = projectId;
    this.form.projectPhaseId = 0;
    this.form.deliverableId = undefined;
    this.phases = [];
    this.deliverables = [];

    if (projectId) {
      this.projectService.getProjectById(projectId).subscribe({
        next: (project) => {
          this.phases = project.phases || [];
        },
        error: (err) => {
          console.error('Error loading phases:', err);
        }
      });
    }
  }

  onPhaseChange(phaseId: number): void {
    this.form.deliverableId = undefined;
    this.deliverables = [];

    const selectedPhase = this.phases.find(p => p.id === phaseId);
    if (selectedPhase) {
      this.deliverables = selectedPhase.deliverables || [];
    }
  }

  onSubmit(): void {
    if (!this.form.title || !this.form.projectPhaseId || !this.form.author) {
      this.error = 'Por favor completa todos los campos requeridos';
      return;
    }

    if (!this.selectedProjectId) {
      this.error = 'Por favor selecciona un proyecto';
      return;
    }

    this.loading = true;
    this.error = '';

    this.microincrementService.create(this.form).subscribe({
      next: (data) => {
        this.success = true;
        this.loading = false;
        setTimeout(() => {
          this.resetForm();
          this.success = false;
        }, 2000);
      },
      error: (err) => {
        this.error = 'Error al crear microincremento. Verifica que la fase exista.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  resetForm(): void {
    this.form = {
      title: '',
      description: '',
      projectPhaseId: 0,
      deliverableId: undefined,
      author: ''
    };
    this.selectedProjectId = null;
    this.phases = [];
    this.deliverables = [];
    this.error = '';
    this.success = false;
  }
}