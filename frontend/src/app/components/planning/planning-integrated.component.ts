import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PlanningService } from '../../planning.service';

interface Iteracion {
  id: number;
  nombre: string;
  faseOpenUP: string;
  fechaInicio: string;
  fechaFin: string;
  objetivo: string;
  capacidadEquipoHoras: number;
  puntosEstimados: number;
  puntosCompletados: number;
  tareas: Tarea[];
}

interface Tarea {
  id: number;
  nombre: string;
  descripcion: string;
  faseProyecto: string;
  estado: 'NoIniciada' | 'EnProgreso' | 'Completada' | 'Bloqueada';
  fechaInicio?: string;
  fechaFin?: string;
  asignadoA?: string;
  porcentajeCompletado: number;
  bloqueos?: string;
}

@Component({
  selector: 'app-planning-integrated',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './planning-integrated.component.html',
  styleUrl: './planning-integrated.component.scss'
})
export class PlanningIntegratedComponent implements OnInit {
  iteraciones: Iteracion[] = [];
  velocidad: number = 0;
  loading = false;
  projectId: number | null = null;
  
  // Modales
  showIterationModal = false;
  showTaskModal = false;
  editingIteration: Iteracion | null = null;
  currentIterationId: number | null = null;
  
  // Formularios
  nuevaIteracion: any = {
    nombre: '',
    faseOpenUP: '',
    fechaInicio: '',
    fechaFin: '',
    objetivo: '',
    capacidadEquipoHoras: 0,
    puntosEstimados: 0
  };
  
  nuevaTarea: Tarea = {
    id: 0,
    nombre: '',
    descripcion: '',
    faseProyecto: '',
    estado: 'NoIniciada',
    porcentajeCompletado: 0
  };

  editingTask: Tarea | null = null;

  constructor(
    private planningService: PlanningService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.projectId = params['projectId'] ? +params['projectId'] : null;
      this.cargarDatos();
    });
  }

  cargarDatos() {
    this.loading = true;
    this.planningService.getIteraciones(this.projectId || undefined).subscribe({
      next: (data: any[]) => {
        this.iteraciones = data.map(it => ({
          ...it,
          tareas: it.tareas || []
        }));
        this.loading = false;
      },
      error: () => this.loading = false
    });
    
    this.planningService.getVelocidad(this.projectId || undefined).subscribe({
      next: (data: any) => this.velocidad = data.velocidadPromedio
    });
  }

  // Gestión de iteraciones
  abrirModalIteracion(iteracion?: Iteracion) {
    if (iteracion) {
      this.editingIteration = iteracion;
      this.nuevaIteracion = { ...iteracion };
    } else {
      this.editingIteration = null;
      this.resetFormIteracion();
    }
    this.showIterationModal = true;
  }

  guardarIteracion() {
    if (!this.nuevaIteracion.nombre || !this.nuevaIteracion.faseOpenUP) {
      alert('⚠️ Por favor completa los campos obligatorios');
      return;
    }

    const iteracionData = { ...this.nuevaIteracion, projectId: this.projectId };
    this.planningService.crearIteracion(iteracionData).subscribe({
      next: () => {
        alert('✅ Iteración guardada correctamente');
        this.cargarDatos();
        this.cerrarModalIteracion();
      },
      error: (err) => alert('❌ Error al guardar: ' + err.message)
    });
  }

  cerrarModalIteracion() {
    this.showIterationModal = false;
    this.editingIteration = null;
    this.resetFormIteracion();
  }

  resetFormIteracion() {
    this.nuevaIteracion = {
      nombre: '',
      faseOpenUP: '',
      fechaInicio: '',
      fechaFin: '',
      objetivo: '',
      capacidadEquipoHoras: 0,
      puntosEstimados: 0
    };
  }

  // Gestión de tareas
  abrirModalTarea(iteracionId: number, tarea?: Tarea) {
    this.currentIterationId = iteracionId;
    
    if (tarea) {
      this.editingTask = tarea;
      this.nuevaTarea = { ...tarea };
    } else {
      this.editingTask = null;
      this.resetFormTarea();
    }
    this.showTaskModal = true;
  }

  guardarTarea() {
    if (!this.nuevaTarea.nombre) {
      alert('⚠️ El nombre de la tarea es obligatorio');
      return;
    }

    const iteracion = this.iteraciones.find(it => it.id === this.currentIterationId);
    if (!iteracion) return;

    if (this.editingTask) {
      // Editar tarea existente
      const index = iteracion.tareas.findIndex(t => t.id === this.editingTask!.id);
      if (index !== -1) {
        iteracion.tareas[index] = { ...this.nuevaTarea };
      }
    } else {
      // Nueva tarea
      const newId = Math.max(0, ...iteracion.tareas.map(t => t.id)) + 1;
      iteracion.tareas.push({ ...this.nuevaTarea, id: newId });
    }

    // Recalcular progreso de la iteración
    this.recalcularProgresoIteracion(iteracion);
    
    // Guardar cambios en el backend
    this.planningService.actualizarIteracion(iteracion.id, iteracion).subscribe({
      next: () => {
        alert('✅ Tarea guardada correctamente');
        this.cerrarModalTarea();
      },
      error: (err) => {
        alert('❌ Error al guardar la tarea: ' + err.message);
        // Revertir cambios locales si falla (opcional, pero recomendado)
        this.cargarDatos(); 
      }
    });
  }

  eliminarTarea(iteracionId: number, tareaId: number) {
    if (!confirm('¿Estás seguro de eliminar esta tarea?')) return;

    const iteracion = this.iteraciones.find(it => it.id === iteracionId);
    if (!iteracion) return;

    iteracion.tareas = iteracion.tareas.filter(t => t.id !== tareaId);
    this.recalcularProgresoIteracion(iteracion);
    
    // Guardar cambios en el backend
    this.planningService.actualizarIteracion(iteracion.id, iteracion).subscribe({
      next: () => {
        alert('✅ Tarea eliminada');
      },
      error: (err) => {
        alert('❌ Error al eliminar la tarea: ' + err.message);
        this.cargarDatos();
      }
    });
  }

  recalcularProgresoIteracion(iteracion: Iteracion) {
    if (iteracion.tareas.length === 0) {
      iteracion.puntosCompletados = 0;
      return;
    }

    const promedioCompletado = iteracion.tareas.reduce((sum, t) => sum + t.porcentajeCompletado, 0) / iteracion.tareas.length;
    iteracion.puntosCompletados = Math.round((promedioCompletado / 100) * iteracion.puntosEstimados);
  }

  cerrarModalTarea() {
    this.showTaskModal = false;
    this.editingTask = null;
    this.currentIterationId = null;
    this.resetFormTarea();
  }

  resetFormTarea() {
    this.nuevaTarea = {
      id: 0,
      nombre: '',
      descripcion: '',
      faseProyecto: '',
      estado: 'NoIniciada',
      porcentajeCompletado: 0
    };
  }

  // Utilidades
  getTotalTareas(): number {
    return this.iteraciones.reduce((sum, it) => sum + it.tareas.length, 0);
  }

  getBadgeClass(fase: string): string {
    const map: any = {
      'Inicio': 'badge-inicio',
      'Elaboración': 'badge-elaboracion',
      'Construcción': 'badge-construccion',
      'Transición': 'badge-transicion'
    };
    return map[fase] || '';
  }

  getEstadoLabel(estado: string): string {
    const labels: any = {
      'NoIniciada': 'No iniciada',
      'EnProgreso': 'En progreso',
      'Completada': 'Completada',
      'Bloqueada': 'Bloqueada'
    };
    return labels[estado] || estado;
  }

  getEstadoClass(estado: string): string {
    const classes: any = {
      'NoIniciada': 'estado-no-iniciada',
      'EnProgreso': 'estado-en-progreso',
      'Completada': 'estado-completada',
      'Bloqueada': 'estado-bloqueada'
    };
    return classes[estado] || '';
  }

  getProgresoIteracion(iteracion: Iteracion): number {
    if (iteracion.puntosEstimados === 0) return 0;
    return Math.round((iteracion.puntosCompletados / iteracion.puntosEstimados) * 100);
  }

  calcularVelocidadIteracion(iteracion: Iteracion): number {
    const dias = this.calcularDias(iteracion.fechaInicio, iteracion.fechaFin);
    return dias > 0 ? Math.round(iteracion.puntosCompletados / dias * 10) / 10 : 0;
  }

  calcularDias(inicio: string, fin: string): number {
    const diff = new Date(fin).getTime() - new Date(inicio).getTime();
    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  }
}
