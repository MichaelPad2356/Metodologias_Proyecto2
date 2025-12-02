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
    return ['Admin', 'Tester'].includes(this.currentUserRole);
  }

  canDeleteProject(): boolean {
    return ['Admin'].includes(this.currentUserRole);
  }
  
  // HU-013: Matriz de permisos
  canEditStatus(currentStatus: string): boolean {
    if (this.currentUserRole === 'Admin') return true;
    if (this.currentUserRole === 'Developer' && currentStatus === 'Pending') return true; // Dev can start review? Maybe not.
    // Let's stick to specific actions
    return false;
  }
}