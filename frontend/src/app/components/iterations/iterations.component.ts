import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { IterationService } from '../../services/iteration.service';
import { Iteration, IterationTask, CreateIterationRequest, CreateIterationTaskRequest } from '../../models/project.model';
import { UpdateIterationTaskRequest, TASK_STATUS_LABELS, TaskStatus } from '../../models/iteration.model';

@Component({
  selector: 'app-iterations',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './iterations.component.html',
  styleUrls: ['./iterations.component.scss']
})
export class IterationsComponent implements OnInit {
  projectId!: number;
  iterations: Iteration[] = [];
  selectedIteration: Iteration | null = null;
  loading = false;
  error: string | null = null;

  // Modales
  showCreateIterationModal = false;
  showCreateTaskModal = false;
  showEditTaskModal = false;

  // Formularios
  newIteration: CreateIterationRequest = {
    name: '',
    startDate: this.getTodayDate(),
    endDate: this.getTodayDate(),
    observations: ''
  };

  newTask: CreateIterationTaskRequest = {
    name: '',
    description: '',
    assignedTo: ''
  };

  editingTask: IterationTask | null = null;
  taskUpdate: UpdateIterationTaskRequest = {};

  TASK_STATUS_LABELS = TASK_STATUS_LABELS;

  constructor(
    private route: ActivatedRoute,
    private iterationService: IterationService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.projectId = +params['id'];
      if (this.projectId) {
        this.loadIterations();
      }
    });
  }

  private getTodayDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  loadIterations(): void {
    this.loading = true;
    this.error = null;

    this.iterationService.getProjectIterations(this.projectId).subscribe({
      next: (iterations) => {
        this.iterations = iterations;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar las iteraciones';
        this.loading = false;
        console.error('Error:', err);
      }
    });
  }

  selectIteration(iteration: Iteration): void {
    this.selectedIteration = iteration;
  }

  // Crear iteración
  openCreateIterationModal(): void {
    this.showCreateIterationModal = true;
    this.newIteration = {
      name: '',
      startDate: this.getTodayDate(),
      endDate: this.getTodayDate(),
      observations: ''
    };
  }

  closeCreateIterationModal(): void {
    this.showCreateIterationModal = false;
  }

  createIteration(): void {
    if (!this.newIteration.name.trim()) {
      alert('El nombre de la iteración es requerido');
      return;
    }

    this.loading = true;

    this.iterationService.createIteration(this.projectId, this.newIteration).subscribe({
      next: () => {
        this.closeCreateIterationModal();
        this.loadIterations();
      },
      error: (err) => {
        alert('Error al crear la iteración');
        console.error('Error:', err);
        this.loading = false;
      }
    });
  }

  deleteIteration(iteration: Iteration): void {
    if (!confirm(`¿Está seguro de eliminar la iteración "${iteration.name}"?`)) {
      return;
    }

    this.iterationService.deleteIteration(this.projectId, iteration.id).subscribe({
      next: () => {
        if (this.selectedIteration?.id === iteration.id) {
          this.selectedIteration = null;
        }
        this.loadIterations();
      },
      error: (err) => {
        alert('Error al eliminar la iteración');
        console.error('Error:', err);
      }
    });
  }

  // Crear tarea
  openCreateTaskModal(): void {
    if (!this.selectedIteration) return;
    this.showCreateTaskModal = true;
    this.newTask = {
      name: '',
      description: '',
      assignedTo: ''
    };
  }

  closeCreateTaskModal(): void {
    this.showCreateTaskModal = false;
  }

  createTask(): void {
    if (!this.selectedIteration || !this.newTask.name.trim()) {
      alert('El nombre de la tarea es requerido');
      return;
    }

    this.loading = true;

    this.iterationService.createTask(this.projectId, this.selectedIteration.id, this.newTask).subscribe({
      next: () => {
        this.closeCreateTaskModal();
        this.loadIterations();
        if (this.selectedIteration) {
          this.iterationService.getIterationById(this.projectId, this.selectedIteration.id).subscribe({
            next: (iteration) => {
              this.selectedIteration = iteration;
              this.loading = false;
            }
          });
        }
      },
      error: (err) => {
        alert('Error al crear la tarea');
        console.error('Error:', err);
        this.loading = false;
      }
    });
  }

  // Editar tarea
  openEditTaskModal(task: IterationTask): void {
    this.editingTask = task;
    this.taskUpdate = {
      name: task.name,
      description: task.description,
      startDate: task.startDate,
      endDate: task.endDate,
      percentageCompleted: task.percentageCompleted,
      assignedTo: task.assignedTo,
      status: task.status,
      blockages: task.blockages
    };
    this.showEditTaskModal = true;
  }

  closeEditTaskModal(): void {
    this.showEditTaskModal = false;
    this.editingTask = null;
    this.taskUpdate = {};
  }

  updateTask(): void {
    if (!this.selectedIteration || !this.editingTask) return;

    this.loading = true;

    this.iterationService.updateTask(
      this.projectId,
      this.selectedIteration.id,
      this.editingTask.id,
      this.taskUpdate
    ).subscribe({
      next: () => {
        this.closeEditTaskModal();
        this.iterationService.getIterationById(this.projectId, this.selectedIteration!.id).subscribe({
          next: (iteration) => {
            this.selectedIteration = iteration;
            this.loadIterations();
            this.loading = false;
          }
        });
      },
      error: (err) => {
        alert('Error al actualizar la tarea');
        console.error('Error:', err);
        this.loading = false;
      }
    });
  }

  deleteTask(task: IterationTask): void {
    if (!this.selectedIteration) return;

    if (!confirm(`¿Está seguro de eliminar la tarea "${task.name}"?`)) {
      return;
    }

    this.iterationService.deleteTask(this.projectId, this.selectedIteration.id, task.id).subscribe({
      next: () => {
        this.iterationService.getIterationById(this.projectId, this.selectedIteration!.id).subscribe({
          next: (iteration) => {
            this.selectedIteration = iteration;
            this.loadIterations();
          }
        });
      },
      error: (err) => {
        alert('Error al eliminar la tarea');
        console.error('Error:', err);
      }
    });
  }

  getStatusClass(status: TaskStatus): string {
    const classMap: Record<TaskStatus, string> = {
      'NotStarted': 'status-not-started',
      'InProgress': 'status-in-progress',
      'Completed': 'status-completed',
      'Blocked': 'status-blocked'
    };
    return classMap[status] || '';
  }
}
