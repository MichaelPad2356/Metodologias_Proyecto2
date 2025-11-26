import { Routes } from '@angular/router';
import { ProjectListComponent } from './components/project-list/project-list.component';
import { ProjectCreateComponent } from './components/project-create/project-create.component';
import { ProjectDetailComponent } from './components/project-detail/project-detail.component';
import { PlanningComponent } from './planning.component';

export const routes: Routes = [
  // 1. Ruta por defecto (Cuando entras a localhost:4200)
  // Si quieres que el inicio siga siendo tus proyectos, déjalo así.
  // Si prefieres que abra directo el planning, cambia '/projects' por '/planning'.
  { path: '', redirectTo: '/projects', pathMatch: 'full' },

  // 2. Tus rutas originales
  { path: 'projects', component: ProjectListComponent },
  { path: 'projects/new', component: ProjectCreateComponent },
  { path: 'projects/:id', component: ProjectDetailComponent },

  // 3. LA NUEVA RUTA (Debe ir ANTES del comodín **)
  { path: 'planning', component: PlanningComponent },

  // 4. Comodín (Siempre debe ser la ÚLTIMA línea)
  { path: '**', redirectTo: '/projects' }
];