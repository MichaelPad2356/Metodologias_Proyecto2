import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PlanningService {
  private apiUrl = '/api/planning'; // Coincide con el backend

  constructor(private http: HttpClient) {}

  getIteraciones(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  crearIteracion(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }

  getVelocidad(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/velocidad`);
  }
}