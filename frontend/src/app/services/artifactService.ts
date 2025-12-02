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
}