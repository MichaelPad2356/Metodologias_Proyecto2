import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface UserInvitation {
  id: number;
  projectId: number;
  projectName: string;
  projectCode: string;
  role: string;
  status?: string;
  invitedBy?: string;
  invitedAt: string;
  acceptedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class InvitationService {
  private readonly baseUrl = '/api/invitations';
  
  private pendingCountSubject = new BehaviorSubject<number>(0);
  public pendingCount$ = this.pendingCountSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Obtiene las invitaciones pendientes del usuario
   */
  getPendingInvitations(): Observable<UserInvitation[]> {
    return this.http.get<UserInvitation[]>(`${this.baseUrl}/pending`);
  }

  /**
   * Obtiene el conteo de invitaciones pendientes
   */
  getPendingCount(): Observable<number> {
    return this.http.get<number>(`${this.baseUrl}/pending/count`).pipe(
      tap(count => this.pendingCountSubject.next(count))
    );
  }

  /**
   * Actualiza el contador de invitaciones pendientes
   */
  refreshPendingCount(): void {
    this.getPendingCount().subscribe();
  }

  /**
   * Acepta una invitación
   */
  acceptInvitation(invitationId: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/${invitationId}/accept`, {}).pipe(
      tap(() => this.refreshPendingCount())
    );
  }

  /**
   * Rechaza una invitación
   */
  declineInvitation(invitationId: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/${invitationId}/decline`, {}).pipe(
      tap(() => this.refreshPendingCount())
    );
  }

  /**
   * Obtiene todas las invitaciones (historial)
   */
  getAllInvitations(): Observable<UserInvitation[]> {
    return this.http.get<UserInvitation[]>(`${this.baseUrl}/all`);
  }
}
