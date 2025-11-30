import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProjectService } from '../../services/project.service';

interface ProgresoFase {
  fase: string;
  porcentaje: number;
  completadas: number;
  total: number;
}

interface ProgresoProyecto {
  progresoTotal: number;
  totalTareas: number;
  tareasCompletadas: number;
  progresosPorFase: ProgresoFase[];
}

@Component({
  selector: 'app-project-progress',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './project-progress.component.html',
  styleUrl: './project-progress.component.scss'
})
export class ProjectProgressComponent implements OnInit, OnChanges {
  @Input() projectId!: number;
  
  progress: ProgresoProyecto | null = null;
  loading = false;
  error = '';

  constructor(private projectService: ProjectService) {}

  ngOnInit(): void {
    console.log('[ProjectProgress] ngOnInit called with projectId:', this.projectId);
    if (this.projectId) {
      this.loadProgress();
    } else {
      console.warn('[ProjectProgress] No projectId provided!');
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['projectId'] && !changes['projectId'].firstChange && this.projectId) {
      this.loadProgress();
    }
  }

  // Método público para refrescar desde fuera
  refresh(): void {
    if (this.projectId) {
      this.loadProgress();
    }
  }

  loadProgress(): void {
    console.log('[ProjectProgress] Loading progress for project', this.projectId);
    this.loading = true;
    this.error = '';
    
    this.projectService.getProjectProgress(this.projectId).subscribe({
      next: (data) => {
        console.log('[ProjectProgress] Progress data received:', data);
        this.progress = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar el progreso del proyecto';
        console.error('[ProjectProgress] Error loading progress:', err);
        this.loading = false;
      }
    });
  }

  getProgressBarClass(percentage: number): string {
    if (percentage === 100) return 'progress-complete';
    if (percentage >= 75) return 'progress-high';
    if (percentage >= 50) return 'progress-medium';
    if (percentage >= 25) return 'progress-low';
    return 'progress-minimal';
  }

  getProgressWidth(percentage: number): string {
    return `${Math.min(percentage, 100)}%`;
  }
}
