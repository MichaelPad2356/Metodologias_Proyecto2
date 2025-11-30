export enum ArtifactStatus {
  Pending,
  InReview,
  Approved,
}

export enum ArtifactType {
  // Fase Inicio
  VisionDocument,
  StakeholderList,
  InitialRiskList,
  InitialProjectPlan,
  HighLevelUseCaseModel,
  
  // Fase Elaboración
  SoftwareArchitectureDocument,
  DetailedUseCaseModel,
  RefinedRiskList,
  IterationPlan,
  
  // Fase Construcción
  SourceCode,
  UnitTestReport,
  IntegrationTestReport,
  UserGuide,
  
  // Fase Transición (HU-009)
  UserManual,           // Manual de Usuario
  TechnicalManual,      // Manual Técnico
  DeploymentPlan,       // Plan de Despliegue
  ClosureDocument,      // Documento de Cierre
  FinalBuild,           // Build Final
  BetaTestReport,       // Reporte de Pruebas Beta
  
  Other,
}

// Mapeo de tipos de artefactos a nombres legibles en español
export const ArtifactTypeLabels: { [key: number]: string } = {
  [ArtifactType.VisionDocument]: 'Documento de Visión',
  [ArtifactType.StakeholderList]: 'Lista de Interesados',
  [ArtifactType.InitialRiskList]: 'Lista de Riesgos Inicial',
  [ArtifactType.InitialProjectPlan]: 'Plan de Proyecto Inicial',
  [ArtifactType.HighLevelUseCaseModel]: 'Modelo de Casos de Uso de Alto Nivel',
  [ArtifactType.SoftwareArchitectureDocument]: 'Documento de Arquitectura de Software',
  [ArtifactType.DetailedUseCaseModel]: 'Modelo de Casos de Uso Detallado',
  [ArtifactType.RefinedRiskList]: 'Lista de Riesgos Refinada',
  [ArtifactType.IterationPlan]: 'Plan de Iteración',
  [ArtifactType.SourceCode]: 'Código Fuente',
  [ArtifactType.UnitTestReport]: 'Reporte de Pruebas Unitarias',
  [ArtifactType.IntegrationTestReport]: 'Reporte de Pruebas de Integración',
  [ArtifactType.UserGuide]: 'Guía de Usuario',
  [ArtifactType.UserManual]: 'Manual de Usuario',
  [ArtifactType.TechnicalManual]: 'Manual Técnico',
  [ArtifactType.DeploymentPlan]: 'Plan de Despliegue',
  [ArtifactType.ClosureDocument]: 'Documento de Cierre',
  [ArtifactType.FinalBuild]: 'Build Final',
  [ArtifactType.BetaTestReport]: 'Reporte de Pruebas Beta',
  [ArtifactType.Other]: 'Otro',
};

// Artefactos obligatorios por fase
export const MandatoryArtifactsByPhase: { [phase: string]: ArtifactType[] } = {
  'Inicio': [
    ArtifactType.VisionDocument,
    ArtifactType.StakeholderList,
    ArtifactType.InitialRiskList,
  ],
  'Elaboración': [
    ArtifactType.SoftwareArchitectureDocument,
    ArtifactType.DetailedUseCaseModel,
  ],
  'Construcción': [
    ArtifactType.SourceCode,
    ArtifactType.UnitTestReport,
  ],
  'Transición': [
    ArtifactType.UserManual,
    ArtifactType.TechnicalManual,
    ArtifactType.DeploymentPlan,
    ArtifactType.ClosureDocument,
    ArtifactType.FinalBuild,
    ArtifactType.BetaTestReport,
  ],
};

// Interface para item de checklist de cierre
export interface ClosureChecklistItem {
  id: number;
  description: string;
  isMandatory: boolean;
  isCompleted: boolean;
  completedDate?: string;
  completedBy?: string;
  notes?: string;
}

// Checklist por defecto para documento de cierre
export const DefaultClosureChecklist: ClosureChecklistItem[] = [
  { id: 1, description: 'Manual de Usuario entregado y aprobado', isMandatory: true, isCompleted: false },
  { id: 2, description: 'Manual Técnico entregado y aprobado', isMandatory: true, isCompleted: false },
  { id: 3, description: 'Plan de Despliegue ejecutado exitosamente', isMandatory: true, isCompleted: false },
  { id: 4, description: 'Build Final generado y verificado', isMandatory: true, isCompleted: false },
  { id: 5, description: 'Pruebas Beta completadas con resultados aceptables', isMandatory: true, isCompleted: false },
  { id: 6, description: 'Capacitación a usuarios finales completada', isMandatory: false, isCompleted: false },
  { id: 7, description: 'Documentación de soporte entregada', isMandatory: false, isCompleted: false },
  { id: 8, description: 'Ambiente de producción configurado', isMandatory: true, isCompleted: false },
  { id: 9, description: 'Plan de contingencia definido', isMandatory: false, isCompleted: false },
  { id: 10, description: 'Aceptación formal del cliente obtenida', isMandatory: true, isCompleted: false },
];

export interface ArtifactVersion {
  id: number;
  versionNumber: number;
  author: string;
  observations?: string;  // HU-010: Descripción de cambios
  content?: string;
  originalFileName?: string;
  contentType?: string;  // HU-010: Tipo MIME
  fileSize?: number;  // HU-010: Tamaño en bytes
  createdAt: Date;
  downloadUrl?: string;
}

// HU-010: Interface para comparación de versiones
export interface VersionComparison {
  version1: ArtifactVersion;
  version2: ArtifactVersion;
  differences: string[];
}

// HU-010: Interface para historial exportable
export interface VersionHistoryExport {
  artifactType: string;
  projectName: string;
  phaseName: string;
  exportedAt: Date;
  versions: VersionHistoryItem[];
}

export interface VersionHistoryItem {
  versionNumber: number;
  author: string;
  observations?: string;
  createdAt: Date;
  fileName?: string;
  fileSize?: number;
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
  
  // Campos específicos para Build Final (HU-009)
  buildIdentifier?: string;
  buildDownloadUrl?: string;
  
  // Campos para Documento de Cierre (HU-009)
  closureChecklistJson?: string;
  closureChecklist?: ClosureChecklistItem[];
}