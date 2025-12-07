import { Injectable, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from './auth.service';

// Roles globales del sistema
export type SystemRole = 'Administrador' | 'Usuario';

// Roles en proyectos (HU-025)
export type ProjectRole = 'Autor' | 'Revisor' | 'Product Owner' | 'Scrum Master' | 'Desarrollador' | 'Tester' | 'Administrador';

// Mapeo de roles del sistema a permisos
type PermissionRole = 'Admin' | 'ProjectManager' | 'Developer' | 'Tester' | 'Stakeholder';

/**
 * HU-013: Matriz de permisos por rol
 * 
 * | Rol           | Crear | Editar | Aprobar | Cambiar Estado | Comentar | Eliminar |
 * |---------------|-------|--------|---------|----------------|----------|----------|
 * | Administrador | ✅    | ✅     | ✅      | ✅             | ✅       | ✅       |
 * | Product Owner | ✅    | ✅     | ✅      | ✅             | ✅       | ❌       |
 * | Scrum Master  | ✅    | ✅     | ✅      | ✅             | ✅       | ❌       |
 * | Revisor       | ❌    | ❌     | ✅      | ✅             | ✅       | ❌       |
 * | Autor         | ✅    | ✅     | ❌      | ✅ (limitado)  | ✅       | ❌       |
 * | Desarrollador | ✅    | ✅     | ❌      | ✅ (limitado)  | ✅       | ❌       |
 * | Tester        | ✅*   | ✅*    | ❌      | ✅ (defectos)  | ✅       | ❌       |
 * 
 * * Tester puede crear/editar defectos y casos de prueba
 */

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private authService = inject(AuthService);
  private systemRole: SystemRole = 'Usuario';
  private projectRole: ProjectRole | null = null;
  private roleSubject = new BehaviorSubject<PermissionRole>('Developer');
  public role$ = this.roleSubject.asObservable();

  constructor() {
    // Suscribirse a cambios del usuario autenticado
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.setSystemRole(user.role || 'Usuario');
      } else {
        this.systemRole = 'Usuario';
        this.projectRole = null;
        this.updateEffectiveRole();
      }
    });
  }

  setSystemRole(role: string): void {
    this.systemRole = role === 'Administrador' ? 'Administrador' : 'Usuario';
    this.updateEffectiveRole();
  }

  setProjectRole(role: ProjectRole | null): void {
    this.projectRole = role;
    this.updateEffectiveRole();
  }

  private updateEffectiveRole(): void {
    // Si es admin del sistema, tiene todos los permisos
    if (this.systemRole === 'Administrador') {
      this.roleSubject.next('Admin');
      return;
    }

    // Mapear rol de proyecto a permisos
    if (this.projectRole) {
      const roleMap: Record<ProjectRole, PermissionRole> = {
        'Administrador': 'Admin',
        'Product Owner': 'ProjectManager',
        'Scrum Master': 'ProjectManager',
        'Autor': 'Developer',
        'Desarrollador': 'Developer',
        'Revisor': 'Stakeholder', // Revisor tiene permisos limitados: solo aprobar/comentar
        'Tester': 'Tester'
      };
      this.roleSubject.next(roleMap[this.projectRole] || 'Developer');
    } else {
      this.roleSubject.next('Developer');
    }
  }

  get currentUserRole(): PermissionRole {
    return this.roleSubject.value;
  }

  getSystemRole(): SystemRole {
    return this.systemRole;
  }

  getProjectRole(): ProjectRole | null {
    return this.projectRole;
  }

  // =====================================================
  // HU-013: Métodos de permisos basados en matriz de roles
  // =====================================================

  /**
   * ¿Puede crear artefactos?
   * Admin, PO, SM, Autor, Desarrollador: SÍ
   * Revisor, Tester: NO (solo lectura para artefactos normales)
   */
  canCreateArtifact(): boolean {
    if (this.systemRole === 'Administrador') return true;
    
    const allowedRoles: ProjectRole[] = ['Administrador', 'Product Owner', 'Scrum Master', 'Autor', 'Desarrollador'];
    return this.projectRole !== null && allowedRoles.includes(this.projectRole);
  }

  /**
   * ¿Puede editar artefactos?
   * Admin, PO, SM, Autor, Desarrollador: SÍ
   * Revisor: NO (solo puede aprobar/comentar)
   * Tester: NO (solo puede crear defectos)
   */
  canEditArtifact(): boolean {
    if (this.systemRole === 'Administrador') return true;
    
    const allowedRoles: ProjectRole[] = ['Administrador', 'Product Owner', 'Scrum Master', 'Autor', 'Desarrollador'];
    return this.projectRole !== null && allowedRoles.includes(this.projectRole);
  }

  /**
   * ¿Puede revisar/comentar artefactos?
   * TODOS los roles pueden comentar
   */
  canReviewArtifact(): boolean {
    return true; // Todos pueden comentar/revisar
  }

  /**
   * ¿Puede aprobar/rechazar artefactos?
   * Admin, PO, SM, Revisor: SÍ
   * Autor, Desarrollador, Tester: NO
   */
  canApproveArtifact(): boolean {
    if (this.systemRole === 'Administrador') return true;
    
    const allowedRoles: ProjectRole[] = ['Administrador', 'Product Owner', 'Scrum Master', 'Revisor'];
    return this.projectRole !== null && allowedRoles.includes(this.projectRole);
  }

  /**
   * ¿Puede crear defectos?
   * Admin, Tester, Desarrollador: SÍ
   * Otros roles: También pueden reportar defectos
   */
  canCreateDefect(): boolean {
    if (this.systemRole === 'Administrador') return true;
    
    // Todos pueden crear defectos, pero Tester es el rol principal
    return true;
  }

  /**
   * ¿Puede eliminar proyecto?
   * Solo Admin del proyecto o Admin del sistema
   */
  canDeleteProject(): boolean {
    if (this.systemRole === 'Administrador') return true;
    return this.projectRole === 'Administrador';
  }
  
  /**
   * HU-013: ¿Puede cambiar estado de un artefacto?
   * Admin, PO, SM: Todos los estados
   * Revisor: Puede aprobar/rechazar
   * Autor, Desarrollador: Solo mover a revisión
   * Tester: Solo estados de defectos
   */
  canEditStatus(currentStatus?: string): boolean {
    if (this.systemRole === 'Administrador') return true;
    if (this.projectRole === 'Administrador') return true;

    // PO y SM pueden cambiar cualquier estado
    if (this.projectRole === 'Product Owner' || this.projectRole === 'Scrum Master') {
      return true;
    }

    // Revisor puede aprobar/rechazar (cambiar estados de revisión)
    if (this.projectRole === 'Revisor') {
      const reviewStates = ['InReview', 'PendingApproval', 'Submitted'];
      return !currentStatus || reviewStates.includes(currentStatus);
    }

    // Autor y Desarrollador pueden mover a revisión
    if (this.projectRole === 'Autor' || this.projectRole === 'Desarrollador') {
      const editableStates = ['Draft', 'Pending', 'InProgress'];
      return !currentStatus || editableStates.includes(currentStatus);
    }

    // Tester puede cambiar estados de defectos
    if (this.projectRole === 'Tester') {
      return true; // Los defectos tienen su propio flujo
    }

    return false;
  }

  /**
   * ¿Puede eliminar defectos?
   * Solo Admin
   */
  canDeleteDefect(): boolean {
    if (this.systemRole === 'Administrador') return true;
    return this.projectRole === 'Administrador';
  }

  /**
   * ¿Puede exportar historial?
   * Admin, PO, SM: SÍ
   */
  canExportHistory(): boolean {
    if (this.systemRole === 'Administrador') return true;
    
    const allowedRoles: ProjectRole[] = ['Administrador', 'Product Owner', 'Scrum Master'];
    return this.projectRole !== null && allowedRoles.includes(this.projectRole);
  }

  /**
   * ¿Puede gestionar miembros del proyecto?
   * Admin, PO: SÍ
   */
  canManageMembers(): boolean {
    if (this.systemRole === 'Administrador') return true;
    
    const allowedRoles: ProjectRole[] = ['Administrador', 'Product Owner'];
    return this.projectRole !== null && allowedRoles.includes(this.projectRole);
  }

  /**
   * ¿Puede crear iteraciones?
   * Admin, SM, PO: SÍ
   */
  canManageIterations(): boolean {
    if (this.systemRole === 'Administrador') return true;
    
    const allowedRoles: ProjectRole[] = ['Administrador', 'Scrum Master', 'Product Owner'];
    return this.projectRole !== null && allowedRoles.includes(this.projectRole);
  }

  /**
   * ¿Puede cerrar proyecto?
   * Admin, PO: SÍ
   */
  canCloseProject(): boolean {
    if (this.systemRole === 'Administrador') return true;
    
    const allowedRoles: ProjectRole[] = ['Administrador', 'Product Owner'];
    return this.projectRole !== null && allowedRoles.includes(this.projectRole);
  }

  /**
   * ¿Puede marcar artefactos como obligatorios?
   * Admin, PO: SÍ
   */
  canSetArtifactRequired(): boolean {
    if (this.systemRole === 'Administrador') return true;
    
    const allowedRoles: ProjectRole[] = ['Administrador', 'Product Owner'];
    return this.projectRole !== null && allowedRoles.includes(this.projectRole);
  }

  /**
   * ¿Puede reasignar entregables entre fases?
   * Admin, PO: SÍ
   */
  canReassignDeliverable(): boolean {
    if (this.systemRole === 'Administrador') return true;
    
    const allowedRoles: ProjectRole[] = ['Administrador', 'Product Owner'];
    return this.projectRole !== null && allowedRoles.includes(this.projectRole);
  }

  /**
   * ¿Puede ver todo el contenido? (modo lectura)
   * TODOS: SÍ
   */
  canViewContent(): boolean {
    return true;
  }

  /**
   * ¿Es solo lectura para este usuario en el proyecto?
   * Revisor: Solo puede aprobar/comentar
   * Tester: Solo puede gestionar defectos
   */
  isReadOnlyForArtifacts(): boolean {
    if (this.systemRole === 'Administrador') return false;
    
    const readOnlyRoles: ProjectRole[] = ['Revisor', 'Tester'];
    return this.projectRole !== null && readOnlyRoles.includes(this.projectRole);
  }

  getCurrentUserRole(): string {
    return this.currentUserRole;
  }

  /**
   * Obtiene un mensaje descriptivo del permiso denegado
   */
  getPermissionDeniedMessage(action: string): string {
    const roleLabel = this.projectRole || 'Usuario';
    return `No tienes permiso para ${action}. Tu rol actual es: ${roleLabel}`;
  }
}
