import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { PlanningService } from './planning.service';

@Component({
  selector: 'app-planning',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  providers: [PlanningService],
  styles: [`
    :host {
      display: block;
      background-color: #f3f4f6; /* Fondo gris muy suave */
      min-height: 100vh;
      font-family: 'Inter', system-ui, -apple-system, sans-serif;
      color: #1f2937;
    }
    
    .container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 3rem 1.5rem;
    }

    /* --- HEADER --- */
    .header {
      margin-bottom: 2.5rem;
      border-bottom: 1px solid #e5e7eb;
      padding-bottom: 1.5rem;
    }
    .header h1 {
      color: #111827;
      font-size: 2.25rem;
      margin: 0 0 0.5rem 0;
      font-weight: 800;
      letter-spacing: -0.025em;
    }
    .header p {
      color: #6b7280;
      font-size: 1.1rem;
      margin: 0;
    }

    /* --- CARDS GENERAL --- */
    .card {
      background: white;
      border-radius: 16px;
      box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.03), 0 4px 6px -2px rgba(0, 0, 0, 0.01);
      padding: 2rem;
      margin-bottom: 2rem;
      border: 1px solid #f3f4f6;
    }
    .card-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 1.5rem;
    }
    .card h3 {
      color: #374151;
      margin: 0;
      font-size: 1.25rem;
      font-weight: 600;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    /* --- METRICS BOX (VELOCIDAD) --- */
    .metrics-container {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 1.5rem;
    }
    .metric-card {
      background: linear-gradient(145deg, #2563eb, #1d4ed8);
      color: white;
      border-radius: 12px;
      padding: 1.5rem;
      position: relative;
      overflow: hidden;
      display: flex;
      flex-direction: column;
      justify-content: center;
    }
    .metric-value {
      font-size: 3rem;
      font-weight: 800;
      line-height: 1;
      margin-bottom: 0.5rem;
    }
    .metric-label {
      font-size: 1rem;
      font-weight: 500;
      opacity: 0.9;
    }
    .metric-sub {
      font-size: 0.875rem;
      opacity: 0.7;
      margin-top: 0.25rem;
    }
    
    /* --- FORMULARIOS --- */
    .form-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 1.5rem;
      margin-bottom: 1.5rem;
    }
    .form-group label {
      display: block;
      margin-bottom: 0.5rem;
      color: #4b5563;
      font-weight: 500;
      font-size: 0.9rem;
    }
    .form-control {
      width: 100%;
      padding: 0.75rem 1rem;
      border: 1px solid #d1d5db;
      border-radius: 8px;
      font-size: 0.95rem;
      transition: all 0.2s;
      background-color: #f9fafb;
      box-sizing: border-box;
    }
    .form-control:focus {
      outline: none;
      border-color: #2563eb;
      background-color: white;
      box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
    }
    textarea.form-control {
      min-height: 100px;
      resize: vertical;
    }

    /* --- BOTONES --- */
    .btn-primary {
      background-color: #2563eb;
      color: white;
      border: none;
      padding: 0.75rem 1.5rem;
      border-radius: 8px;
      font-size: 0.95rem;
      font-weight: 600;
      cursor: pointer;
      transition: background-color 0.2s, transform 0.1s;
      box-shadow: 0 4px 6px -1px rgba(37, 99, 235, 0.2);
    }
    .btn-primary:hover {
      background-color: #1d4ed8;
      box-shadow: 0 6px 8px -1px rgba(37, 99, 235, 0.3);
    }
    .btn-primary:active {
      transform: translateY(1px);
    }

    /* --- TABLA --- */
    .table-responsive {
      overflow-x: auto;
      border-radius: 8px;
      border: 1px solid #e5e7eb;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      background: white;
    }
    th {
      background-color: #f9fafb;
      color: #6b7280;
      font-weight: 600;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      padding: 1rem 1.5rem;
      text-align: left;
      border-bottom: 1px solid #e5e7eb;
    }
    td {
      padding: 1rem 1.5rem;
      border-bottom: 1px solid #f3f4f6;
      color: #374151;
      vertical-align: middle;
    }
    tr:last-child td {
      border-bottom: none;
    }
    tr:hover td {
      background-color: #f8fafc;
    }

    /* --- BADGES --- */
    .badge {
      display: inline-flex;
      align-items: center;
      padding: 0.25rem 0.75rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 600;
    }
    .badge-inicio { background-color: #eff6ff; color: #1e40af; border: 1px solid #dbeafe; }
    .badge-elaboracion { background-color: #fff7ed; color: #9a3412; border: 1px solid #ffedd5; }
    .badge-construccion { background-color: #f0fdf4; color: #166534; border: 1px solid #dcfce7; }
    .badge-transicion { background-color: #faf5ff; color: #6b21a8; border: 1px solid #f3e8ff; }
  `],
  template: `
    <div class="container">
      
      <!-- HEADER -->
      <div class="header">
        <h1>Planificación de Iteraciones</h1>
        <p>Gestiona el ciclo de vida del proyecto y monitorea la velocidad del equipo.</p>
      </div>

      <div class="metrics-container" style="margin-bottom: 2rem;">
        <!-- MÉTRICAS DE VELOCIDAD -->
        <div class="metric-card">
          <div class="metric-value">{{ velocidad | number:'1.0-1' }} <span style="font-size: 1.5rem; opacity: 0.7;">pts</span></div>
          <div class="metric-label">Velocidad Promedio</div>
          <div class="metric-sub">Puntos completados por iteración histórica</div>
        </div>

        <!-- TARJETA INFORMATIVA (TIP) -->
        <div class="card" style="margin-bottom: 0; display: flex; flex-direction: column; justify-content: center; background: #f8fafc; border: 1px dashed #cbd5e1; box-shadow: none;">
          <h4 style="margin: 0 0 0.5rem 0; color: #475569;">💡 Sugerencia de Planificación</h4>
          <p style="margin: 0; color: #64748b; font-size: 0.95rem; line-height: 1.5;">
            Basado en el rendimiento histórico, se recomienda que el alcance de la próxima iteración 
            esté cerca de los <strong>{{ velocidad | number:'1.0-0' }} puntos</strong> para asegurar una entrega realista.
          </p>
        </div>
      </div>

      <!-- FORMULARIO DE CREACIÓN -->
      <div class="card">
        <div class="card-header">
          <h3>
            <span style="background:#e0f2fe; color:#0369a1; padding:4px 8px; border-radius:6px; font-size:0.9rem;">Nuevo</span>
            Definir Iteración
          </h3>
        </div>
        
        <div class="form-grid">
          <div class="form-group">
            <label>Nombre</label>
            <input [(ngModel)]="nueva.nombre" type="text" class="form-control" placeholder="Ej: Iteración 1 - MVP">
          </div>

          <div class="form-group">
            <label>Fase OpenUP</label>
            <select [(ngModel)]="nueva.faseOpenUP" class="form-control">
              <option value="">Seleccionar fase...</option>
              <option value="Inicio">Inicio</option>
              <option value="Elaboración">Elaboración</option>
              <option value="Construcción">Construcción</option>
              <option value="Transición">Transición</option>
            </select>
          </div>

          <div class="form-group">
            <label>Fecha Inicio</label>
            <input [(ngModel)]="nueva.fechaInicio" type="date" class="form-control">
          </div>

          <div class="form-group">
            <label>Fecha Fin</label>
            <input [(ngModel)]="nueva.fechaFin" type="date" class="form-control">
          </div>

          <div class="form-group">
            <label>Capacidad Equipo (Horas)</label>
            <input [(ngModel)]="nueva.capacidadEquipoHoras" type="number" class="form-control" placeholder="0">
          </div>

          <div class="form-group">
            <label>Puntos Estimados (Alcance)</label>
            <input [(ngModel)]="nueva.puntosEstimados" type="number" class="form-control" placeholder="0">
          </div>
        </div>

        <div class="form-group">
          <label>Objetivo Principal</label>
          <textarea [(ngModel)]="nueva.objetivo" class="form-control" placeholder="Describe brevemente qué se logrará en esta iteración..."></textarea>
        </div>

        <div style="text-align: right; margin-top: 2rem;">
          <button (click)="guardar()" class="btn-primary">
            Guardar Planificación
          </button>
        </div>
      </div>

      <!-- TABLA DE ITERACIONES -->
      <div class="card">
        <div class="card-header">
          <h3>📋 Historial de Iteraciones</h3>
        </div>
        
        <div *ngIf="iteraciones.length === 0" style="text-align: center; color: #9ca3af; padding: 3rem; background: #f9fafb; border-radius: 8px;">
          <p style="margin: 0; font-size: 1.1rem;">No hay iteraciones registradas aún.</p>
          <small>Usa el formulario de arriba para crear la primera.</small>
        </div>

        <div class="table-responsive" *ngIf="iteraciones.length > 0">
          <table>
            <thead>
              <tr>
                <th>Iteración</th>
                <th>Fase</th>
                <th>Cronograma</th>
                <th>Capacidad</th>
                <th>Puntos (Plan/Real)</th>
                <th>Objetivo</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let it of iteraciones">
                <td style="font-weight: 600; color: #111827;">{{ it.nombre }}</td>
                <td>
                  <span class="badge" [ngClass]="getBadgeClass(it.faseOpenUP)">
                    {{ it.faseOpenUP }}
                  </span>
                </td>
                <td style="font-size: 0.9rem; white-space: nowrap;">
                  {{ it.fechaInicio | date:'dd MMM' }} - {{ it.fechaFin | date:'dd MMM yyyy' }}
                </td>
                <td>{{ it.capacidadEquipoHoras }} h</td>
                <td>
                  <span style="font-weight: 700; color: #2563eb;">{{ it.puntosEstimados }}</span> 
                  <span style="color: #9ca3af; margin: 0 4px;">/</span> 
                  <span [style.color]="it.puntosCompletados ? '#059669' : '#9ca3af'">{{ it.puntosCompletados || '-' }}</span>
                </td>
                <td style="color: #6b7280; font-size: 0.9rem; max-width: 300px; line-height: 1.4;">
                  {{ it.objetivo }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  `
})
export class PlanningComponent implements OnInit {
  iteraciones: any[] = [];
  velocidad: number = 0;

  nueva: any = {
    nombre: '', 
    faseOpenUP: '', 
    fechaInicio: '', 
    fechaFin: '', 
    objetivo: '', 
    capacidadEquipoHoras: null, 
    puntosEstimados: null
  };

  constructor(private srv: PlanningService) {}

  ngOnInit() {
    this.cargar();
  }

  cargar() {
    this.srv.getIteraciones().subscribe(data => this.iteraciones = data);
    this.srv.getVelocidad().subscribe(data => this.velocidad = data.velocidadPromedio);
  }

  guardar() {
    if(!this.nueva.nombre || !this.nueva.faseOpenUP) {
      alert("⚠️ Por favor completa el Nombre y la Fase para continuar.");
      return;
    }

    this.srv.crearIteracion(this.nueva).subscribe({
      next: () => {
        alert('✅ Planificación guardada correctamente');
        this.cargar();
        this.resetForm();
      },
      error: (err) => alert('❌ Error al guardar: ' + err.message)
    });
  }

  resetForm() {
    this.nueva = { 
      nombre: '', faseOpenUP: '', fechaInicio: '', fechaFin: '', 
      objetivo: '', capacidadEquipoHoras: null, puntosEstimados: null 
    };
  }

  getBadgeClass(fase: string): string {
    switch(fase) {
      case 'Inicio': return 'badge-inicio';
      case 'Elaboración': return 'badge-elaboracion';
      case 'Construcción': return 'badge-construccion';
      case 'Transición': return 'badge-transicion';
      default: return '';
    }
  }
}