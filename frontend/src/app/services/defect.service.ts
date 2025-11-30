import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Defect } from '../models/defect.model';

@Injectable({
  providedIn: 'root'
})
export class DefectService {
  // >> CAMBIO AQUÍ: Poner el puerto correcto (5277) <<
  private apiUrl = 'http://localhost:5277/api/defects'; 

  constructor(private http: HttpClient) { }

  getDefects(projectId?: number): Observable<Defect[]> {
    const url = projectId ? `${this.apiUrl}?projectId=${projectId}` : this.apiUrl;
    return this.http.get<Defect[]>(url);
  }

  createDefect(defect: Defect): Observable<Defect> {
    return this.http.post<Defect>(this.apiUrl, defect);
  }
}