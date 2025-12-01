export enum ArtifactStatus {
  Pending,
  InReview,
  Approved,
}

export enum ArtifactType {
  VisionDocument,
  StakeholderList,
  InitialRiskList,
  InitialProjectPlan,
  HighLevelUseCaseModel,
  Other,
}

export interface ArtifactVersion {
  id: number;
  versionNumber: number;
  author: string;
  content?: string;
  originalFileName?: string;
  createdAt: Date;
  downloadUrl?: string;
}

export interface WorkflowStep {
  id: number;
  name: string;
  order: number;
}

export interface Workflow {
  id: number;
  name: string;
  steps: WorkflowStep[];
}

export interface Artifact {
  id: number;
  name: string;
  description: string;
  isMandatory: boolean;

  // AGREGAR ESTOS CAMPOS NUEVOS:
  workflowId?: number;
  workflow?: Workflow;
  currentStepId?: number;
  currentStep?: WorkflowStep;

  // ... el resto de tus campos (type, author, etc.) ...
  type: any;
  author: string;
  // etc...
  typeName: string;
  projectPhaseId: number;
  status: ArtifactStatus;
  statusName: string;
  createdAt: Date;
  versions: ArtifactVersion[];
}