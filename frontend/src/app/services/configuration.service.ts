import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  RoleDefinition,
  StageDefinition,
  ArtifactTypeDefinition,
  CustomFieldDefinition,
  ConfigurationVersion,
  CreateRoleRequest,
  CreateStageRequest,
  CreateArtifactTypeRequest,
  CreateCustomFieldRequest,
  SaveConfigVersionRequest
} from '../models/configuration.model';

@Injectable({
  providedIn: 'root'
})
export class ConfigurationService {
  private readonly apiUrl = '/api/configuration';

  constructor(private http: HttpClient) {}

  // ==================== ROLES ====================
  getAllRoles(): Observable<RoleDefinition[]> {
    return this.http.get<RoleDefinition[]>(`${this.apiUrl}/roles`);
  }

  createRole(request: CreateRoleRequest): Observable<RoleDefinition> {
    return this.http.post<RoleDefinition>(`${this.apiUrl}/roles`, request);
  }

  updateRole(id: number, request: Partial<CreateRoleRequest>): Observable<RoleDefinition> {
    return this.http.put<RoleDefinition>(`${this.apiUrl}/roles/${id}`, request);
  }

  deleteRole(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/roles/${id}`);
  }

  // ==================== STAGES ====================
  getAllStages(): Observable<StageDefinition[]> {
    return this.http.get<StageDefinition[]>(`${this.apiUrl}/stages`);
  }

  createStage(request: CreateStageRequest): Observable<StageDefinition> {
    return this.http.post<StageDefinition>(`${this.apiUrl}/stages`, request);
  }

  updateStage(id: number, request: Partial<CreateStageRequest>): Observable<StageDefinition> {
    return this.http.put<StageDefinition>(`${this.apiUrl}/stages/${id}`, request);
  }

  deleteStage(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/stages/${id}`);
  }

  // ==================== ARTIFACT TYPES ====================
  getAllArtifactTypes(): Observable<ArtifactTypeDefinition[]> {
    return this.http.get<ArtifactTypeDefinition[]>(`${this.apiUrl}/artifact-types`);
  }

  createArtifactType(request: CreateArtifactTypeRequest): Observable<ArtifactTypeDefinition> {
    return this.http.post<ArtifactTypeDefinition>(`${this.apiUrl}/artifact-types`, request);
  }

  updateArtifactType(id: number, request: Partial<CreateArtifactTypeRequest>): Observable<ArtifactTypeDefinition> {
    return this.http.put<ArtifactTypeDefinition>(`${this.apiUrl}/artifact-types/${id}`, request);
  }

  deleteArtifactType(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/artifact-types/${id}`);
  }

  // ==================== CUSTOM FIELDS ====================
  getAllCustomFields(): Observable<CustomFieldDefinition[]> {
    return this.http.get<CustomFieldDefinition[]>(`${this.apiUrl}/custom-fields`);
  }

  createCustomField(request: CreateCustomFieldRequest): Observable<CustomFieldDefinition> {
    return this.http.post<CustomFieldDefinition>(`${this.apiUrl}/custom-fields`, request);
  }

  updateCustomField(id: number, request: Partial<CreateCustomFieldRequest>): Observable<CustomFieldDefinition> {
    return this.http.put<CustomFieldDefinition>(`${this.apiUrl}/custom-fields/${id}`, request);
  }

  deleteCustomField(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/custom-fields/${id}`);
  }

  // ==================== VERSIONING ====================
  getAllVersions(): Observable<ConfigurationVersion[]> {
    return this.http.get<ConfigurationVersion[]>(`${this.apiUrl}/versions`);
  }

  saveVersion(request: SaveConfigVersionRequest): Observable<ConfigurationVersion> {
    return this.http.post<ConfigurationVersion>(`${this.apiUrl}/save-version`, request);
  }

  restoreVersion(versionId: number): Observable<{ message: string; restoredVersion: number }> {
    return this.http.post<{ message: string; restoredVersion: number }>(
      `${this.apiUrl}/${versionId}/restore`,
      {}
    );
  }
}
