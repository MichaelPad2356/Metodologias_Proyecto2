// ============================================
// HU-025: Modelos de Miembros de Proyecto
// HU-020: Modelos de Movimientos de Entregables
// ============================================

export interface ProjectMember {
  id: number;
  projectId: number;
  userEmail: string;
  userName?: string;
  role: string;
  status: string;
  invitedAt: string;
  acceptedAt?: string;
  invitedBy?: string;
}

// Roles disponibles (deben coincidir con el backend)
export const AVAILABLE_ROLES = [
  'Autor',
  'Revisor', 
  'Product Owner',
  'Scrum Master',
  'Desarrollador',
  'Tester',
  'Administrador'
] as const;

export type ProjectRole = typeof AVAILABLE_ROLES[number];

export interface InviteMemberRequest {
  userEmail: string;
  userName?: string;
  role: string;
}

export interface UpdateMemberRoleRequest {
  role: string;
}

// HU-020: Movimiento de Entregables
export interface DeliverableMovement {
  id: number;
  deliverableId: number;
  fromPhaseId: number;
  toPhaseId: number;
  movedAt: string;
  movedBy: string;
  reason: string;
  confirmed: boolean;
}

export interface ReassignDeliverableRequest {
  toPhaseId: number;
  reason: string;
  movedBy: string;
}

export interface ConfirmMovementRequest {
  movedBy: string;
}
