import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { IterationService } from '../../services/iteration.service';
import { ProjectService } from '../../services/project.service';
import { 
  Iteration, 
  IterationTask, 
  CreateIterationRequest, 
  UpdateIterationRequest,
  CreateIterationTaskRequest,
  UpdateIterationTaskRequest,
  TaskStatus,
  TASK_STATUS_LABELS,
  TASK_STATUS_COLORS
} from '../../models/iteration.model';
import { Project, ProjectPhase } from '../../models/project.model';

@Component({
  selector: 'app-iteration-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './iteration-management.component.html',
  styleUrl: './iteration-management.component.scss'
})
export class IterationManagementComponent implements OnInit {
  projectId!: number;
  project: Project | null = null;
  iterations: Iteration[] = [];
  phases: ProjectPhase[] = [];
  
  loading = false;
  error = '';
  
  // Modal state
  showIterationModal = false;
  showTaskModal = false;
  editingIteration: Iteration | null = null;
  editingTask: IterationTask | null = null;
  selectedIteration: Iteration | null = null;
  
  // Form data
  iterationForm: CreateIterationRequest | UpdateIterationRequest = {
    name: '',
    startDate: '',
    endDate: '',
    blockages: '',
    observations: ''
  };
  
  taskForm: CreateIterationTaskRequest | UpdateIterationTaskRequest = {
    name: '',
    description: '',
    startDate: '',
    endDate: '',
    percentageCompleted: 0,
    assignedTo: '',
    status: TaskStatus.NotStarted,
    projectPhaseId: undefined,
    blockages: ''
  };
  
  // Enum helpers
  TaskStatus = TaskStatus;
  taskStatusLabels = TASK_STATUS_LABELS;
  taskStatusColors = TASK_STATUS_COLORS;
  taskStatuses = [
    TaskStatus.NotStarted,
    TaskStatus.InProgress,
    TaskStatus.Completed,
    TaskStatus.Blocked
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private iterationService: IterationService,
    private projectService: ProjectService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.projectId = +params['id'];
      if (this.projectId) {
        this.loadProject();
        this.loadIterations();
      }
    });
  }

  loadProject(): void {
    this.projectService.getProjectById(this.projectId).subscribe({
      next: (project) => {
        this.project = project;
        this.loadPhases();
      },
      error: (err) => {
        this.error = 'Error al cargar el proyecto';
        console.error('Error loading project:', err);
      }
    });
  }

  loadPhases(): void {
    this.projectService.getProjectPhases(this.projectId).subscribe({
      next: (phases) => {
        this.phases = phases;
      },
      error: (err) => {
        console.error('Error loading phases:', err);
      }
    });
  }

  loadIterations(): void {
    this.loading = true;
    this.error = '';
    
    this.iterationService.getProjectIterations(this.projectId).subscribe({
      next: (data) => {
        this.iterations = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar las iteraciones';
        console.error('Error loading iterations:', err);
        this.loading = false;
      }
    });
  }

  // Iteration CRUD
  openNewIterationModal(): void {
    this.editingIteration = null;
    this.iterationForm = {
      name: '',
      startDate: '',
      endDate: '',
      blockages: '',
      observations: ''
    };
    this.showIterationModal = true;
  }

  openEditIterationModal(iteration: Iteration): void {
    this.editingIteration = iteration;
    this.iterationForm = {
      name: iteration.name,
      startDate: iteration.startDate,
      endDate: iteration.endDate,
      blockages: iteration.blockages || '',
      observations: iteration.observations || ''
    };
    this.showIterationModal = true;
  }

  closeIterationModal(): void {
    this.showIterationModal = false;
    this.editingIteration = null;
  }

  saveIteration(): void {
    if (this.editingIteration) {
      // Update
      this.iterationService.updateIteration(
        this.projectId, 
        this.editingIteration.id, 
        this.iterationForm as UpdateIterationRequest
      ).subscribe({
        next: () => {
          this.loadIterations();
          this.closeIterationModal();
        },
        error: (err) => {
          alert('Error al actualizar la iteración');
          console.error('Error updating iteration:', err);
        }
      });
    } else {
      // Create
      this.iterationService.createIteration(
        this.projectId, 
        this.iterationForm as CreateIterationRequest
      ).subscribe({
        next: () => {
          this.loadIterations();
          this.closeIterationModal();
        },
        error: (err) => {
          alert('Error al crear la iteración');
          console.error('Error creating iteration:', err);
        }
      });
    }
  }

  deleteIteration(iteration: Iteration): void {
    if (!confirm(`¿Eliminar la iteración "${iteration.name}"?`)) return;
    
    this.iterationService.deleteIteration(this.projectId, iteration.id).subscribe({
      next: () => {
        this.loadIterations();
      },
      error: (err) => {
        alert('Error al eliminar la iteración');
        console.error('Error deleting iteration:', err);
      }
    });
  }

  // Task CRUD
  openNewTaskModal(iteration: Iteration): void {
    this.selectedIteration = iteration;
    this.editingTask = null;
    this.taskForm = {
      name: '',
      description: '',
      startDate: '',
      endDate: '',
      percentageCompleted: 0,
      assignedTo: '',
      status: TaskStatus.NotStarted,
      projectPhaseId: undefined,
      blockages: ''
    };
    this.showTaskModal = true;
  }

  openEditTaskModal(iteration: Iteration, task: IterationTask): void {
    this.selectedIteration = iteration;
    this.editingTask = task;
    this.taskForm = {
      name: task.name,
      description: task.description || '',
      startDate: task.startDate || '',
      endDate: task.endDate || '',
      percentageCompleted: task.percentageCompleted,
      assignedTo: task.assignedTo || '',
      status: task.status,
      projectPhaseId: task.projectPhaseId,
      blockages: task.blockages || ''
    };
    this.showTaskModal = true;
  }

  closeTaskModal(): void {
    this.showTaskModal = false;
    this.editingTask = null;
    this.selectedIteration = null;
  }

  saveTask(): void {
    if (!this.selectedIteration) return;
    
    if (this.editingTask) {
      // Update
      this.iterationService.updateTask(
        this.projectId,
        this.selectedIteration.id,
        this.editingTask.id,
        this.taskForm as UpdateIterationTaskRequest
      ).subscribe({
        next: () => {
          this.loadIterations();
          this.closeTaskModal();
        },
        error: (err) => {
          alert('Error al actualizar la tarea');
          console.error('Error updating task:', err);
        }
      });
    } else {
      // Create
      this.iterationService.createTask(
        this.projectId,
        this.selectedIteration.id,
        this.taskForm as CreateIterationTaskRequest
      ).subscribe({
        next: () => {
          this.loadIterations();
          this.closeTaskModal();
        },
        error: (err) => {
          alert('Error al crear la tarea');
          console.error('Error creating task:', err);
        }
      });
    }
  }

  deleteTask(iteration: Iteration, task: IterationTask): void {
    if (!confirm(`¿Eliminar la tarea "${task.name}"?`)) return;
    
    this.iterationService.deleteTask(this.projectId, iteration.id, task.id).subscribe({
      next: () => {
        this.loadIterations();
      },
      error: (err) => {
        alert('Error al eliminar la tarea');
        console.error('Error deleting task:', err);
      }
    });
  }

  // Helper methods
  getStatusColor(status: TaskStatus): string {
    return this.taskStatusColors[status] || '#666';
  }

  getStatusLabel(status: TaskStatus): string {
    return this.taskStatusLabels[status] || status;
  }

  getPhaseName(phaseId: number | null | undefined): string {
    if (!phaseId) return 'Sin fase asignada';
    const phase = this.phases.find(p => p.id === phaseId);
    return phase ? phase.name : 'Fase desconocida';
  }

  formatDate(dateString: string | null | undefined): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('es-ES');
  }

  trackByIterationId(index: number, iteration: Iteration): number {
    return iteration.id;
  }

  trackByTaskId(index: number, task: IterationTask): number {
    return task.id;
  }

  trackByPhaseId(index: number, phase: ProjectPhase): number {
    return phase.id;
  }
}
