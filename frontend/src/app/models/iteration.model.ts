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
  projectPhaseId?: number;
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
  blockages?: string;
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
  percentageCompleted?: number;
  assignedTo?: string;
  status?: TaskStatus;
  projectPhaseId?: number;
  blockages?: string;
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
  projectPhaseId?: number;
}

export interface ProjectProgress {
  projectId: number;
  overallPercentage: number;
  phaseProgress: PhaseProgress[];
  recentIterations: IterationSummary[];
}

export interface PhaseProgress {
  phaseId: number;
  phaseName: string;
  percentageCompleted: number;
  totalTasks: number;
  completedTasks: number;
}

export interface IterationSummary {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
  percentageCompleted: number;
  totalTasks: number;
  completedTasks: number;
}

export enum TaskStatus {
  NotStarted = 'NotStarted',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Blocked = 'Blocked'
}

export const TASK_STATUS_LABELS: Record<TaskStatus, string> = {
  [TaskStatus.NotStarted]: 'No iniciada',
  [TaskStatus.InProgress]: 'En progreso',
  [TaskStatus.Completed]: 'Completada',
  [TaskStatus.Blocked]: 'Bloqueada'
};

export const TASK_STATUS_COLORS: Record<TaskStatus, string> = {
  [TaskStatus.NotStarted]: '#6c757d',
  [TaskStatus.InProgress]: '#0d6efd',
  [TaskStatus.Completed]: '#198754',
  [TaskStatus.Blocked]: '#dc3545'
};
