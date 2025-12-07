// ============================================
// HU-019: Modelos de Plantillas OpenUP
// ============================================

export interface OpenUpTemplate {
  id: number;
  name: string;
  description: string;
  version: string;
  configurationJson: string;
  isDefault: boolean;
  createdAt: string;
  createdBy: string;
  phases: TemplatePhase[];
}

export interface TemplatePhase {
  id: number;
  templateId: number;
  name: string;
  order: number;
  mandatoryArtifactTypesJson: string; // JSON array of artifact type ids
}

export interface CreateTemplateRequest {
  name: string;
  description: string;
  version: string;
  configurationJson: string;
  isDefault: boolean;
  createdBy: string;
  phases: CreateTemplatePhaseRequest[];
}

export interface CreateTemplatePhaseRequest {
  name: string;
  order: number;
  mandatoryArtifactTypesJson: string;
}

export interface UpdateTemplateRequest {
  name?: string;
  description?: string;
  version?: string;
  configurationJson?: string;
  isDefault?: boolean;
}

export interface TemplateComparison {
  template1: OpenUpTemplate;
  template2: OpenUpTemplate;
  differences: TemplateDifference[];
}

export interface TemplateDifference {
  field: string;
  template1Value: string;
  template2Value: string;
}
