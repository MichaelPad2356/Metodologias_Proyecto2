import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  // SIMULACIÓN: Cambia esto para probar diferentes roles ('Admin', 'Tester', 'Developer')
<<<<<<< HEAD
  currentUserRole: string = 'Tester';
=======
  currentUserRole: string = 'Tester'; 
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d

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
<<<<<<< HEAD

=======
  
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
  // HU-013: Matriz de permisos
  canEditStatus(currentStatus: string): boolean {
    if (this.currentUserRole === 'Admin') return true;
    if (this.currentUserRole === 'Developer' && currentStatus === 'Pending') return true;
    if (this.currentUserRole === 'Tester' && currentStatus === 'InReview') return true;
    return false;
  }
<<<<<<< HEAD
}
=======
}
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
