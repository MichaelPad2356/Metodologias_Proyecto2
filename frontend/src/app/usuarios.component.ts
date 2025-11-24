import { Component, OnInit } from '@angular/core';
import { UsuariosService } from './usuarios.service';

import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  providers: [UsuariosService],
  template: `
    <h2>Usuarios</h2>
    <ul>
      <li *ngFor="let usuario of usuarios">{{ usuario.nombre }}</li>
    </ul>
    <div *ngIf="error" style="color:red">Error: {{ error }}</div>
  `
})
export class UsuariosComponent implements OnInit {
  usuarios: any[] = [];
  error: string = '';

  constructor(private usuariosService: UsuariosService) {}

  ngOnInit() {
    this.usuariosService.getUsuarios().subscribe({
      next: (data) => this.usuarios = data,
      error: (err) => this.error = err.message || 'No se pudo conectar al backend'
    });
  }
}
