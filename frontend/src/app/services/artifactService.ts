import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Artifact, ArtifactVersion, VersionComparison } from '../models/artifact.model';

export interface TransitionArtifactsResponse {
  phaseId: number;
  artifacts: Artifact[];
  mandatoryTypes: { type: number; typeName: string }[];
  missingMandatory: { type: number; typeName: string }[];
  canClose: boolean;
}

export interface ClosureValidationResponse {
  canClose: boolean;
  missingArtifacts: string[];
  pendingApproval: { type: number; typeName: string }[];
  checklistValidation: {
    isValid: boolean;
    pendingItems: string[];
  };
}

export interface UpdateArtifactDto {
  status?: number;
  buildIdentifier?: string;
  buildDownloadUrl?: string;
  closureChecklistJson?: string;
}

// HU-010: Response para historial de versiones
export interface VersionsResponse {
  artifactId: number;
  artifactType: string;
  projectName: string;
  phaseName: string;
  totalVersions: number;
  versions: ArtifactVersion[];
}

@Injectable({
  providedIn: 'root'
})
export class ArtifactService {
  private apiUrl = '/api/artifacts';

  constructor(private http: HttpClient) { }

  getArtifactsForPhase(phaseId: number): Observable<Artifact[]> {
    return this.http.get<Artifact[]>(`${this.apiUrl}/phase/${phaseId}`);
  }

  createArtifact(data: FormData): Observable<Artifact> {
    return this.http.post<Artifact>(this.apiUrl, data);
  }

  updateArtifact(id: number, dto: UpdateArtifactDto): Observable<Artifact> {
    return this.http.put<Artifact>(`${this.apiUrl}/${id}`, dto);
  }

  addVersion(artifactId: number, data: FormData): Observable<ArtifactVersion> {
    return this.http.post<ArtifactVersion>(`${this.apiUrl}/${artifactId}/versions`, data);
  }

  // HU-009: Métodos específicos para fase de Transición
  getTransitionArtifacts(projectId: number): Observable<TransitionArtifactsResponse> {
    return this.http.get<TransitionArtifactsResponse>(`${this.apiUrl}/transition/${projectId}`);
  }

  validateProjectClosure(projectId: number): Observable<ClosureValidationResponse> {
    return this.http.post<ClosureValidationResponse>(`${this.apiUrl}/validate-closure/${projectId}`, {});
  }

  // HU-010: Métodos para control de versiones
  
  // Obtener todas las versiones de un artefacto
  getVersions(artifactId: number): Observable<VersionsResponse> {
    return this.http.get<VersionsResponse>(`${this.apiUrl}/${artifactId}/versions`);
  }

  // Comparar metadatos de dos versiones
  compareVersions(artifactId: number, v1: number, v2: number): Observable<VersionComparison> {
    return this.http.get<VersionComparison>(`${this.apiUrl}/${artifactId}/versions/compare?v1=${v1}&v2=${v2}`);
  }

  // Exportar historial de versiones como JSON
  exportVersionHistory(artifactId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${artifactId}/history/export`, {
      responseType: 'blob'
    });
  }

  // Descargar archivo de una versión específica
  downloadVersion(artifactId: number, versionNumber: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${artifactId}/versions/${versionNumber}/download`, {
      responseType: 'blob'
    });
  }
}
