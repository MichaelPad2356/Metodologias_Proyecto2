import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Defect } from '../models/defect.model';

@Injectable({
  providedIn: 'root'
})
export class DefectService {
  private apiUrl = '/api/defects';

  constructor(private http: HttpClient) { }

  getDefects(projectId?: number): Observable<Defect[]> {
    const url = projectId ? `${this.apiUrl}?projectId=${projectId}` : this.apiUrl;
    return this.http.get<Defect[]>(url);
  }

  getDefect(id: number): Observable<Defect> {
    return this.http.get<Defect>(`${this.apiUrl}/${id}`);
  }

  createDefect(defect: Defect): Observable<Defect> {
    return this.http.post<Defect>(this.apiUrl, defect);
  }

  updateDefect(id: number, defect: Defect): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, defect);
  }

  deleteDefect(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
