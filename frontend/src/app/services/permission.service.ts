import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  // Simulated user role - in a real app, this would come from authentication
  private currentUserRole: string = 'admin';

  constructor() { }

  canCreateArtifact(): boolean {
    return ['admin', 'developer', 'analyst'].includes(this.currentUserRole);
  }

  canApproveArtifact(): boolean {
    return ['admin', 'manager'].includes(this.currentUserRole);
  }

  canCreateDefect(): boolean {
    return ['admin', 'developer', 'tester', 'analyst'].includes(this.currentUserRole);
  }

  canEditDefect(): boolean {
    return ['admin', 'developer', 'tester'].includes(this.currentUserRole);
  }

  canEditStatus(): boolean {
    return ['admin', 'manager', 'developer'].includes(this.currentUserRole);
  }

  canDeleteDefect(): boolean {
    return ['admin'].includes(this.currentUserRole);
  }

  canExportHistory(): boolean {
    return ['admin', 'manager', 'analyst'].includes(this.currentUserRole);
  }

  getCurrentUserRole(): string {
    return this.currentUserRole;
  }

  setCurrentUserRole(role: string): void {
    this.currentUserRole = role;
  }
}
