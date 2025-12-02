export enum ArtifactStatus {
  Pending,
  InReview,
  Approved,
}

export enum ArtifactType {
  // Inception
  VisionDocument,
  StakeholderList,
  InitialRiskList,
  InitialProjectPlan,
  HighLevelUseCaseModel,
  
  // Elaboration
  DetailedUseCaseModel,
  DomainModel,
  SupplRequirements,
  NonFunctionalReqs,
  ArchitectureDoc,
  TechnicalDiagrams,
  IterationPlan,
  UIPrototype,

  // Construction
  DetailedDesignModel,
  SourceCode,
  TestCases,
  TestResults,
  IterationLog,

  // Transition
  UserManual,
  TechnicalManual,
  DeploymentPlan,
  ClosureDoc,
  FinalBuild,
  BetaTestReport,

  Other,
}

export interface ArtifactVersion {
  id: number;
  versionNumber: number;
  author: string;
  content?: string;
  originalFileName?: string;
  repositoryUrl?: string;
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
  assignedTo?: string;
  createdAt: Date;
  versions: ArtifactVersion[];
}