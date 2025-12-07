// ============================================
// HU-026: Modelos de Cierre de Proyecto
// ============================================

export interface ClosureValidation {
  projectId: number;
  projectName: string;
  isValid: boolean;
  checks: ClosureCheck[];
  missingRequirements: string[];
}

export interface ClosureCheck {
  name: string;
  description: string;
  passed: boolean;
  details: string;
}

export interface CloseProjectRequest {
  closedBy: string;
  closureNotes?: string;
}

export interface ForceCloseRequest {
  closedBy: string;
  justification: string;
}

export interface ClosureResult {
  success: boolean;
  message: string;
  closureDocumentId?: number;
  archivedAt?: string;
}
