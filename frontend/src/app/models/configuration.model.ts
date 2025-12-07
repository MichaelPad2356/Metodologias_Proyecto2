// ============================================
// HU-018: Modelos de Configuración Global
// ============================================

export interface RoleDefinition {
  id: number;
  name: string;
  description: string;
  permissions: string; // JSON string
  isSystem: boolean;
  createdAt: string;
}

export interface StageDefinition {
  id: number;
  name: string;
  description: string;
  order: number;
  isSystem: boolean;
  createdAt: string;
}

export interface ArtifactTypeDefinition {
  id: number;
  name: string;
  description: string;
  defaultMandatory: boolean;
  applicableStages: string; // JSON array of stage ids
  isSystem: boolean;
  createdAt: string;
}

export interface CustomFieldDefinition {
  id: number;
  name: string;
  fieldType: CustomFieldType;
  options: string; // JSON for dropdown options
  isRequired: boolean;
  artifactTypeId?: number;
  createdAt: string;
}

export enum CustomFieldType {
  Text = 'Text',
  Number = 'Number',
  Date = 'Date',
  Dropdown = 'Dropdown',
  Checkbox = 'Checkbox',
  TextArea = 'TextArea'
}

export interface ConfigurationVersion {
  id: number;
  versionNumber: number;
  snapshotJson: string;
  createdBy: string;
  createdAt: string;
  description: string;
}

// DTOs para creación
export interface CreateRoleRequest {
  name: string;
  description: string;
  permissions: string;
}

export interface CreateStageRequest {
  name: string;
  description: string;
  order: number;
}

export interface CreateArtifactTypeRequest {
  name: string;
  description: string;
  defaultMandatory: boolean;
  applicableStages: string;
}

export interface CreateCustomFieldRequest {
  name: string;
  fieldType: CustomFieldType;
  options?: string;
  isRequired: boolean;
  artifactTypeId?: number;
}

export interface SaveConfigVersionRequest {
  createdBy: string;
  description: string;
}
