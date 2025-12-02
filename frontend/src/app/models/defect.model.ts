export enum DefectSeverity {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}

export enum DefectStatus {
  New = 'New',
  Assigned = 'Assigned',
  Fixed = 'Fixed',
  Closed = 'Closed'
}

export interface Defect {
  id: number;
  title: string;
  description: string;
  severity: DefectSeverity;
  status: DefectStatus;
  projectId: number;
  artifactId?: number;
  reportedBy: string;
  assignedTo?: string;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateDefectDto {
  title: string;
  description: string;
  severity: DefectSeverity;
  projectId: number;
  artifactId?: number;
  reportedBy: string;
  assignedTo?: string;
}

export const DefectSeverityLabels: { [key: string]: string } = {
  [DefectSeverity.Low]: 'Bajo',
  [DefectSeverity.Medium]: 'Medio',
  [DefectSeverity.High]: 'Alto',
  [DefectSeverity.Critical]: 'Crítico'
};

export const DefectStatusLabels: { [key: string]: string } = {
  [DefectStatus.New]: 'Nuevo',
  [DefectStatus.Assigned]: 'Asignado',
  [DefectStatus.Fixed]: 'Corregido',
  [DefectStatus.Closed]: 'Cerrado'
};
