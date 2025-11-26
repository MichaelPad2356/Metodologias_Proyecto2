import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  Iteration, 
  CreateIterationRequest, 
  UpdateIterationRequest,
  IterationTask,
  CreateIterationTaskRequest,
  UpdateIterationTaskRequest,
  ProjectProgress
} from '../models/project.model';

@Injectable({
  providedIn: 'root'
})
export class IterationService {
  private readonly apiUrl = '/api/projects';

  constructor(private http: HttpClient) {}

  /**
   * Obtiene todas las iteraciones de un proyecto
   */
  getProjectIterations(projectId: number): Observable<Iteration[]> {
    return this.http.get<Iteration[]>(`${this.apiUrl}/${projectId}/iterations`);
  }

  /**
   * Obtiene una iteración específica por ID
   */
  getIterationById(projectId: number, iterationId: number): Observable<Iteration> {
    return this.http.get<Iteration>(`${this.apiUrl}/${projectId}/iterations/${iterationId}`);
  }

  /**
   * Crea una nueva iteración
   */
  createIteration(projectId: number, request: CreateIterationRequest): Observable<Iteration> {
    return this.http.post<Iteration>(`${this.apiUrl}/${projectId}/iterations`, request);
  }

  /**
   * Actualiza una iteración existente
   */
  updateIteration(projectId: number, iterationId: number, request: UpdateIterationRequest): Observable<Iteration> {
    return this.http.put<Iteration>(`${this.apiUrl}/${projectId}/iterations/${iterationId}`, request);
  }

  /**
   * Elimina una iteración
   */
  deleteIteration(projectId: number, iterationId: number): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(`${this.apiUrl}/${projectId}/iterations/${iterationId}`);
  }

  /**
   * Crea una nueva tarea en una iteración
   */
  createTask(projectId: number, iterationId: number, request: CreateIterationTaskRequest): Observable<IterationTask> {
    return this.http.post<IterationTask>(
      `${this.apiUrl}/${projectId}/iterations/${iterationId}/tasks`,
      request
    );
  }

  /**
   * Actualiza una tarea
   */
  updateTask(projectId: number, iterationId: number, taskId: number, request: UpdateIterationTaskRequest): Observable<IterationTask> {
    return this.http.put<IterationTask>(
      `${this.apiUrl}/${projectId}/iterations/${iterationId}/tasks/${taskId}`,
      request
    );
  }

  /**
   * Elimina una tarea
   */
  deleteTask(projectId: number, iterationId: number, taskId: number): Observable<{ success: boolean }> {
    return this.http.delete<{ success: boolean }>(
      `${this.apiUrl}/${projectId}/iterations/${iterationId}/tasks/${taskId}`
    );
  }

  /**
   * Obtiene el progreso general del proyecto
   */
  getProjectProgress(projectId: number): Observable<ProjectProgress> {
    return this.http.get<ProjectProgress>(`${this.apiUrl}/${projectId}/progress`);
  }
}
