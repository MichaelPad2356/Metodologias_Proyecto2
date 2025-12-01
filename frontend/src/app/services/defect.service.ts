import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Defect } from '../models/defect.model';

@Injectable({
  providedIn: 'root'
})
export class DefectService {
<<<<<<< HEAD
  private apiUrl = '/api/defects';
=======
  private apiUrl = '/api/defects';  // Usar URL relativa para que funcione con el proxy
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d

  constructor(private http: HttpClient) { }

  getDefects(projectId?: number): Observable<Defect[]> {
    const url = projectId ? `${this.apiUrl}?projectId=${projectId}` : this.apiUrl;
    return this.http.get<Defect[]>(url);
  }

<<<<<<< HEAD
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
=======
  createDefect(defect: Defect): Observable<Defect> {
    return this.http.post<Defect>(this.apiUrl, defect);
  }
}
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
