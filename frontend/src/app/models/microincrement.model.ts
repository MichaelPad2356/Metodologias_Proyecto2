export interface Microincrement {
  id: number;
  title: string;
  description?: string;
  date: Date;
  author: string;
  projectPhaseId: number;
  deliverableId?: number;
}

export interface CreateMicroincrementRequest {
  title: string;
  description?: string;
  projectPhaseId: number;
  deliverableId?: number;
  author: string;
}

export interface UpdateMicroincrementRequest {
  title?: string;
  description?: string;
  author?: string;
}