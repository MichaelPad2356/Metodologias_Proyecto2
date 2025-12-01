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

  createArtifact(data: FormData): Observable<Artifact> {
    // No se establece Content-Type aquí, el navegador lo hará por nosotros
    // y añadirá el 'boundary' correcto para multipart/form-data.
    return this.http.post<Artifact>(this.apiUrl, data);
  }

  updateStatus(artifactId: number, newStepId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${artifactId}/status`, {
      newStepId: newStepId,
      comments: 'Cambio de estado desde el panel',
      changedBy: 'Usuario Demo'
    });
  }
}