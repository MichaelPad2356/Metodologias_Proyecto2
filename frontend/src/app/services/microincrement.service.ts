import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Microincrement, CreateMicroincrementRequest, UpdateMicroincrementRequest } from '../models/microincrement.model';

@Injectable({
  providedIn: 'root'
})
export class MicroincrementService {
  private apiUrl = 'http://localhost:5277/api/microincrements';

  constructor(private http: HttpClient) { }

  getAll(): Observable<Microincrement[]> {
    return this.http.get<Microincrement[]>(this.apiUrl);
  }

  getById(id: number): Observable<Microincrement> {
    return this.http.get<Microincrement>(`${this.apiUrl}/${id}`);
  }

  getByIteration(phaseId: number): Observable<Microincrement[]> {
    return this.http.get<Microincrement[]>(`${this.apiUrl}/by-iteration/${phaseId}`);
  }

  getByDeliverable(deliverableId: number): Observable<Microincrement[]> {
    return this.http.get<Microincrement[]>(`${this.apiUrl}/by-deliverable/${deliverableId}`);
  }

  getByAuthor(author: string): Observable<Microincrement[]> {
    return this.http.get<Microincrement[]>(`${this.apiUrl}/by-author/${author}`);
  }

  create(request: CreateMicroincrementRequest): Observable<Microincrement> {
    return this.http.post<Microincrement>(this.apiUrl, request);
  }

  update(id: number, request: UpdateMicroincrementRequest): Observable<Microincrement> {
    return this.http.put<Microincrement>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}