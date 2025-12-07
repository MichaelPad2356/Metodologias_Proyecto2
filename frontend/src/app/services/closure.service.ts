import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ClosureValidation,
  CloseProjectRequest,
  ForceCloseRequest,
  ClosureResult
} from '../models/closure.model';

@Injectable({
  providedIn: 'root'
})
export class ClosureService {
  private readonly baseUrl = '/api/projects';

  constructor(private http: HttpClient) {}

  /**
   * Valida si un proyecto puede ser cerrado
   */
  validateClosure(projectId: number): Observable<ClosureValidation> {
    return this.http.get<ClosureValidation>(`${this.baseUrl}/${projectId}/closure/validate`);
  }

  /**
   * Cierra un proyecto (solo si pasa todas las validaciones)
   */
  closeProject(projectId: number, request: CloseProjectRequest): Observable<ClosureResult> {
    return this.http.post<ClosureResult>(`${this.baseUrl}/${projectId}/closure/close`, request);
  }

  /**
   * Fuerza el cierre de un proyecto con justificación
   */
  forceCloseProject(projectId: number, request: ForceCloseRequest): Observable<ClosureResult> {
    return this.http.post<ClosureResult>(`${this.baseUrl}/${projectId}/closure/force-close`, request);
  }
}
