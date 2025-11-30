import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  // SIMULACIÓN: Cambia esto para probar diferentes roles ('Admin', 'Tester', 'Developer')
  currentUserRole: string = 'Tester'; 

  constructor() { }

  canCreateArtifact(): boolean {
    return ['Admin', 'Developer'].includes(this.currentUserRole);
  }

  canApproveArtifact(): boolean {
    return ['Admin', 'ProjectManager'].includes(this.currentUserRole);
  }

  canCreateDefect(): boolean {
    return ['Admin', 'Tester'].includes(this.currentUserRole);
  }
  
  // HU-013: Matriz de permisos
  canEditStatus(currentStatus: string): boolean {
    if (this.currentUserRole === 'Admin') return true;
    if (this.currentUserRole === 'Developer' && currentStatus === 'Pending') return true;
    if (this.currentUserRole === 'Tester' && currentStatus === 'InReview') return true;
    return false;
  }
}