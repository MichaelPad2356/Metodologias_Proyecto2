import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  OpenUpTemplate,
  CreateTemplateRequest,
  UpdateTemplateRequest,
  TemplateComparison
} from '../models/template.model';

@Injectable({
  providedIn: 'root'
})
export class TemplateService {
  private readonly apiUrl = '/api/templates';

  constructor(private http: HttpClient) {}

  /**
   * Obtiene todas las plantillas OpenUP
   */
  getAllTemplates(): Observable<OpenUpTemplate[]> {
    return this.http.get<OpenUpTemplate[]>(this.apiUrl);
  }

  /**
   * Obtiene una plantilla por ID
   */
  getTemplateById(id: number): Observable<OpenUpTemplate> {
    return this.http.get<OpenUpTemplate>(`${this.apiUrl}/${id}`);
  }

  /**
   * Obtiene la plantilla por defecto
   */
  getDefaultTemplate(): Observable<OpenUpTemplate> {
    return this.http.get<OpenUpTemplate>(`${this.apiUrl}/default`);
  }

  /**
   * Crea una nueva plantilla
   */
  createTemplate(request: CreateTemplateRequest): Observable<OpenUpTemplate> {
    return this.http.post<OpenUpTemplate>(this.apiUrl, request);
  }

  /**
   * Actualiza una plantilla existente
   */
  updateTemplate(id: number, request: UpdateTemplateRequest): Observable<OpenUpTemplate> {
    return this.http.put<OpenUpTemplate>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Elimina una plantilla
   */
  deleteTemplate(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
  }

  /**
   * Crea una nueva versión de una plantilla existente
   */
  createVersion(id: number, newVersion: string): Observable<OpenUpTemplate> {
    return this.http.post<OpenUpTemplate>(`${this.apiUrl}/${id}/version`, { newVersion });
  }

  /**
   * Compara dos plantillas
   */
  compareTemplates(id1: number, id2: number): Observable<TemplateComparison> {
    return this.http.get<TemplateComparison>(`${this.apiUrl}/compare/${id1}/${id2}`);
  }

  /**
   * Aplica una plantilla a un proyecto
   */
  applyToProject(templateId: number, projectId: number): Observable<{ message: string; success: boolean }> {
    return this.http.post<{ message: string; success: boolean }>(
      `${this.apiUrl}/${templateId}/apply/${projectId}`,
      {}
    );
  }

  /**
   * Establece una plantilla como la predeterminada
   */
  setAsDefault(id: number): Observable<OpenUpTemplate> {
    return this.http.put<OpenUpTemplate>(`${this.apiUrl}/${id}`, { isDefault: true });
  }
}
