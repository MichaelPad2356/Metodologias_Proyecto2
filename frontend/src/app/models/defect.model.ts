import { Artifact } from "./artifact.model";

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
  id?: number;
  title: string;
  description: string;
  severity: DefectSeverity;
  status: DefectStatus;
  projectId: number;
  artifactId?: number;
  artifact?: Artifact;
  reportedBy?: string;
  assignedTo?: string;
  createdAt?: Date;
<<<<<<< HEAD
}
=======
}
>>>>>>> 472c841cee103fffcd9ca2f9fe1589083cdecf5d
