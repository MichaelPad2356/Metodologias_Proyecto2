export interface Project {
  id: number;
  name: string;
  code: string;
  startDate: string;
  description?: string;
  responsiblePerson?: string;
  tags?: string;
  objetivos?: string;
  alcance?: string;
  cronogramaInicial?: string;
  responsables?: string;
  hitos?: string;
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
  objetivos?: string;
  alcance?: string;
  cronogramaInicial?: string;
  responsables?: string;
  hitos?: string;
}

export interface UpdateProjectRequest {
  name?: string;
  description?: string;
  responsiblePerson?: string;
  tags?: string;
  startDate?: string;
  objetivos?: string;
  alcance?: string;
  cronogramaInicial?: string;
  responsables?: string;
  hitos?: string;
}

export interface ProjectPlanVersion {
  id: number;
  projectId: number;
  version: number;
  objetivos?: string;
  alcance?: string;
  cronogramaInicial?: string;
  responsables?: string;
  hitos?: string;
  observaciones?: string;
  createdBy?: string;
  createdAt: string;
}

export interface CreatePlanVersionRequest {
  observaciones: string;
}

export interface Iteration {
  id: number;
  projectId: number;
  name: string;
  startDate: string;
  endDate: string;
  percentageCompleted: number;
  blockages?: string;
  observations?: string;
  createdAt: string;
  updatedAt?: string;
  tasks: IterationTask[];
}

export interface IterationTask {
  id: number;
  iterationId: number;
  name: string;
  description?: string;
  startDate?: string;
  endDate?: string;
  percentageCompleted: number;
  assignedTo?: string;
  status: TaskStatus;
  blockages?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateIterationRequest {
  name: string;
  startDate: string;
  endDate: string;
  observations?: string;
}

export interface UpdateIterationRequest {
  name?: string;
  startDate?: string;
  endDate?: string;
  percentageCompleted?: number;
  blockages?: string;
  observations?: string;
}

export interface CreateIterationTaskRequest {
  name: string;
  description?: string;
  startDate?: string;
  endDate?: string;
  assignedTo?: string;
}

export interface UpdateIterationTaskRequest {
  name?: string;
  description?: string;
  startDate?: string;
  endDate?: string;
  percentageCompleted?: number;
  assignedTo?: string;
  status?: TaskStatus;
  blockages?: string;
}

export interface ProjectProgress {
  projectId: number;
  projectName: string;
  totalIterations: number;
  completedIterations: number;
  overallProgress: number;
  phaseProgress: PhaseProgress[];
}

export interface PhaseProgress {
  phaseName: string;
  iterationsCount: number;
  averageProgress: number;
}

export type TaskStatus = 'NotStarted' | 'InProgress' | 'Completed' | 'Blocked';

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

export const TASK_STATUS_LABELS: Record<TaskStatus, string> = {
  'NotStarted': 'No iniciada',
  'InProgress': 'En progreso',
  'Completed': 'Completada',
  'Blocked': 'Bloqueada'
};
