import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PlanningService {
  private apiUrl = '/api/planning';

  constructor(private http: HttpClient) {}

  getIteraciones(projectId?: number): Observable<any[]> {
    let params = new HttpParams();
    if (projectId) {
      params = params.set('projectId', projectId.toString());
    }
    return this.http.get<any[]>(this.apiUrl, { params });
  }

  crearIteracion(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }

  actualizarIteracion(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data);
  }

  eliminarIteracion(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  getVelocidad(projectId?: number): Observable<any> {
    let params = new HttpParams();
    if (projectId) {
      params = params.set('projectId', projectId.toString());
    }
    return this.http.get<any>(`${this.apiUrl}/velocidad-historica`, { params });
  }

  getProgreso(projectId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/progreso/${projectId}`);
  }
}