import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IterationService } from '../../services/iteration.service';
import { ProjectProgress, PhaseProgress } from '../../models/iteration.model';

@Component({
  selector: 'app-project-progress',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './project-progress.component.html',
  styleUrl: './project-progress.component.scss'
})
export class ProjectProgressComponent implements OnInit {
  @Input() projectId!: number;
  
  progress: ProjectProgress | null = null;
  loading = false;
  error = '';

  constructor(private iterationService: IterationService) {}

  ngOnInit(): void {
    console.log('[ProjectProgress] ngOnInit called with projectId:', this.projectId);
    if (this.projectId) {
      this.loadProgress();
    } else {
      console.warn('[ProjectProgress] No projectId provided!');
    }
  }

  loadProgress(): void {
    console.log('[ProjectProgress] Loading progress for project', this.projectId);
    this.loading = true;
    this.error = '';
    
    this.iterationService.getProjectProgress(this.projectId).subscribe({
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

  // Calcula el total de tareas desde las iteraciones recientes
  getTotalTasks(): number {
    if (!this.progress?.recentIterations) return 0;
    return this.progress.recentIterations.reduce((sum, iter) => sum + iter.totalTasks, 0);
  }

  // Calcula las tareas completadas desde las iteraciones recientes
  getCompletedTasks(): number {
    if (!this.progress?.recentIterations) return 0;
    return this.progress.recentIterations.reduce((sum, iter) => sum + iter.completedTasks, 0);
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

  trackByPhaseId(index: number, phase: PhaseProgress): number {
    return phase.phaseId;
  }
}
