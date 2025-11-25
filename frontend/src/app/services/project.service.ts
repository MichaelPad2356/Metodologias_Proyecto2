import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  Project, 
  ProjectListItem, 
  CreateProjectRequest, 
  UpdateProjectRequest 
} from '../models/project.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private readonly apiUrl = '/api/projects';

  constructor(private http: HttpClient) {}

  /**
   * Obtiene todos los proyectos
   */
  getAllProjects(includeArchived: boolean = false): Observable<ProjectListItem[]> {
    const params = new HttpParams().set('includeArchived', includeArchived.toString());
    return this.http.get<ProjectListItem[]>(this.apiUrl, { params });
  }

  /**
   * Obtiene un proyecto por ID
   */
  getProjectById(id: number): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/${id}`);
  }

  /**
   * Obtiene un proyecto por código
   */
  getProjectByCode(code: string): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/by-code/${code}`);
  }

  /**
   * Crea un nuevo proyecto OpenUP
   */
  createProject(request: CreateProjectRequest): Observable<Project> {
    return this.http.post<Project>(this.apiUrl, request);
  }

  /**
   * Actualiza un proyecto existente
   */
  updateProject(id: number, request: UpdateProjectRequest): Observable<Project> {
    return this.http.put<Project>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Archiva un proyecto
   */
  archiveProject(id: number): Observable<{ message: string; success: boolean }> {
    return this.http.post<{ message: string; success: boolean }>(
      `${this.apiUrl}/${id}/archive`,
      {}
    );
  }

  /**
   * Desarchiva un proyecto
   */
  unarchiveProject(id: number): Observable<{ message: string; success: boolean }> {
    return this.http.post<{ message: string; success: boolean }>(
      `${this.apiUrl}/${id}/unarchive`,
      {}
    );
  }

  /**
   * Elimina permanentemente un proyecto
   */
  deleteProject(id: number): Observable<{ message: string; success: boolean }> {
    return this.http.delete<{ message: string; success: boolean }>(
      `${this.apiUrl}/${id}`
    );
  }
}
