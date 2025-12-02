import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Artifact } from '../models/artifact.model';

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
    // No se establece Content-Type aquí, el navegador lo hará por nosotros
    // y añadirá el 'boundary' correcto para multipart/form-data.
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