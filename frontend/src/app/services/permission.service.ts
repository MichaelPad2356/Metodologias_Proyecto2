import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type UserRole = 'Admin' | 'ProjectManager' | 'Developer' | 'Tester' | 'Stakeholder';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private roleSubject = new BehaviorSubject<UserRole>('Admin');
  public role$ = this.roleSubject.asObservable();

  constructor() { }

  setRole(role: UserRole) {
    this.roleSubject.next(role);
  }

  get currentUserRole(): UserRole {
    return this.roleSubject.value;
  }

  canCreateArtifact(): boolean {
    return ['Admin', 'Developer', 'ProjectManager'].includes(this.currentUserRole);
  }

  canReviewArtifact(): boolean {
    return ['Admin', 'Developer', 'ProjectManager'].includes(this.currentUserRole);
  }

  canApproveArtifact(): boolean {
    return ['Admin', 'ProjectManager'].includes(this.currentUserRole);
  }

  canCreateDefect(): boolean {
    return ['Admin', 'Developer', 'Tester'].includes(this.currentUserRole);
  }

  canDeleteProject(): boolean {
    return ['Admin'].includes(this.currentUserRole);
  }
  
  // HU-013: Matriz de permisos
  canEditStatus(currentStatus?: string): boolean {
    const role = this.currentUserRole;
    
    // Si no se especifica estado, verificamos si el rol tiene permiso general de editar estados
    if (!currentStatus) {
        return ['Admin', 'ProjectManager', 'Developer'].includes(role);
    }

    // Admin puede hacer todo
    if (role === 'Admin') return true;

    // ProjectManager puede aprobar/rechazar
    if (role === 'ProjectManager') return true;

    // Developer puede mover de Pending a InReview
    if (role === 'Developer') {
        if (currentStatus === 'Pending') return true;
        if (currentStatus === 'InReview') return false; 
    }

    return false;
  }

  canDeleteDefect(): boolean {
    return ['Admin'].includes(this.currentUserRole);
  }

  canExportHistory(): boolean {
    return ['Admin', 'ProjectManager'].includes(this.currentUserRole);
  }

  getCurrentUserRole(): string {
    return this.currentUserRole;
  }

  setCurrentUserRole(role: UserRole): void {
    this.setRole(role);
  }
}
