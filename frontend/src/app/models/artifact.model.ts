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

export interface Artifact {
  id: number;
  type: ArtifactType;
  typeName: string;
  projectPhaseId: number;
  isMandatory: boolean;
  status: ArtifactStatus;
  statusName: string;
  createdAt: Date;
  versions: ArtifactVersion[];
}