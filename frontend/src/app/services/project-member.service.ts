import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  ProjectMember,
  InviteMemberRequest,
  UpdateMemberRoleRequest,
  DeliverableMovement,
  ReassignDeliverableRequest,
  ConfirmMovementRequest
} from '../models/project-member.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectMemberService {
  private readonly baseUrl = '/api/projects';

  constructor(private http: HttpClient) {}

  // ==================== PROJECT MEMBERS ====================

  /**
   * Obtiene todos los miembros de un proyecto
   */
  getProjectMembers(projectId: number): Observable<ProjectMember[]> {
    return this.http.get<ProjectMember[]>(`${this.baseUrl}/${projectId}/members`);
  }

  /**
   * Obtiene el rol del usuario actual en un proyecto específico
   */
  getCurrentUserRole(projectId: number, userEmail: string): Observable<string | null> {
    return this.getProjectMembers(projectId).pipe(
      map(members => {
        const member = members.find(m => 
          m.userEmail.toLowerCase() === userEmail.toLowerCase() && 
          m.status === 'Accepted'
        );
        return member ? member.role : null;
      })
    );
  }

  /**
   * Invita a un nuevo miembro al proyecto
   */
  inviteMember(projectId: number, request: InviteMemberRequest): Observable<ProjectMember> {
    return this.http.post<ProjectMember>(`${this.baseUrl}/${projectId}/members/invite`, request);
  }

  /**
   * Actualiza el rol de un miembro
   */
  updateMemberRole(projectId: number, memberId: number, request: UpdateMemberRoleRequest): Observable<ProjectMember> {
    return this.http.put<ProjectMember>(`${this.baseUrl}/${projectId}/members/${memberId}`, request);
  }

  /**
   * Acepta una invitación al proyecto
   */
  acceptInvitation(projectId: number, memberId: number): Observable<ProjectMember> {
    return this.http.post<ProjectMember>(
      `${this.baseUrl}/${projectId}/members/${memberId}/accept`,
      {}
    );
  }

  /**
   * Elimina un miembro del proyecto
   */
  removeMember(projectId: number, memberId: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/${projectId}/members/${memberId}`);
  }
}

@Injectable({
  providedIn: 'root'
})
export class DeliverableService {
  private readonly apiUrl = '/api/deliverables';

  constructor(private http: HttpClient) {}

  /**
   * Reasigna un entregable a otra fase
   */
  reassignDeliverable(deliverableId: number, request: ReassignDeliverableRequest): Observable<DeliverableMovement> {
    return this.http.put<DeliverableMovement>(`${this.apiUrl}/${deliverableId}/reassign`, request);
  }

  /**
   * Confirma el movimiento de un entregable
   */
  confirmMovement(deliverableId: number, request: ConfirmMovementRequest): Observable<{ message: string; success: boolean }> {
    return this.http.post<{ message: string; success: boolean }>(
      `${this.apiUrl}/${deliverableId}/confirm-movement`,
      request
    );
  }

  /**
   * Obtiene el historial de movimientos de un entregable
   */
  getMovementHistory(deliverableId: number): Observable<DeliverableMovement[]> {
    return this.http.get<DeliverableMovement[]>(`${this.apiUrl}/${deliverableId}/movement-history`);
  }
}
