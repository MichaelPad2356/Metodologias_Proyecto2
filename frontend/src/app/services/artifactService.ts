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

  getArtifactsByProject(projectId: number): Observable<Artifact[]> {
    return this.http.get<Artifact[]>(`${this.apiUrl}/project/${projectId}`);
  }

  createArtifact(data: FormData): Observable<Artifact> {
    return this.http.post<Artifact>(this.apiUrl, data);
  }

  addVersion(artifactId: number, data: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/${artifactId}/versions`, data);
  }

  updateArtifactStatus(artifactId: number, status: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${artifactId}/status`, JSON.stringify(status), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  updateStatus(artifactId: number, status: number): Observable<any> {
    // Convert number status to string if backend expects string, or just send number
    // Based on previous code, backend expects string enum name
    const statusMap: { [key: number]: string } = {
      0: 'Pending',
      1: 'InReview',
      2: 'Approved'
    };
    const statusString = statusMap[status] || 'Pending';
    return this.updateArtifactStatus(artifactId, statusString);
  }

  getTransitionArtifacts(projectId: number): Observable<TransitionArtifactsResponse> {
    return this.http.get<TransitionArtifactsResponse>(`${this.apiUrl}/transition/${projectId}`);
  }

  updateArtifact(id: number, dto: UpdateArtifactDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, dto);
  }

  validateProjectClosure(projectId: number): Observable<ClosureValidationResponse> {
    return this.http.post<ClosureValidationResponse>(`${this.apiUrl}/validate-closure/${projectId}`, {});
  }

  compareVersions(artifactId: number, v1Id: number, v2Id: number): Observable<VersionComparison> {
    return this.http.get<VersionComparison>(`${this.apiUrl}/${artifactId}/compare?v1=${v1Id}&v2=${v2Id}`);
  }

  exportVersionHistory(artifactId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${artifactId}/history/export`, { responseType: 'blob' });
  }

  downloadVersion(artifactId: number, versionNumber: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${artifactId}/versions/${versionNumber}/download`, { responseType: 'blob' });
  }
}