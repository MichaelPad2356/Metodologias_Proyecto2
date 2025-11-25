export interface Project {
  id: number;
  name: string;
  code: string;
  startDate: string;
  description?: string;
  responsiblePerson?: string;
  tags?: string;
  status: ProjectStatus;
  createdAt: string;
  archivedAt?: string;
  phases: ProjectPhase[];
}

export interface ProjectPhase {
  id: number;
  name: string;
  order: number;
  status: PhaseStatus;
}

export interface ProjectListItem {
  id: number;
  name: string;
  code: string;
  startDate: string;
  responsiblePerson?: string;
  status: ProjectStatus;
  createdAt: string;
  phaseCount: number;
}

export interface CreateProjectRequest {
  name: string;
  code: string;
  startDate: string;
  description?: string;
  responsiblePerson?: string;
  tags?: string;
}

export interface UpdateProjectRequest {
  name?: string;
  description?: string;
  responsiblePerson?: string;
  tags?: string;
  startDate?: string;
}

export type ProjectStatus = 'Created' | 'Active' | 'Archived' | 'Closed';
export type PhaseStatus = 'NotStarted' | 'InProgress' | 'Completed';

export const PROJECT_STATUS_LABELS: Record<ProjectStatus, string> = {
  'Created': 'Creado',
  'Active': 'Activo',
  'Archived': 'Archivado',
  'Closed': 'Cerrado'
};

export const PHASE_STATUS_LABELS: Record<PhaseStatus, string> = {
  'NotStarted': 'No iniciado',
  'InProgress': 'En progreso',
  'Completed': 'Completado'
};
