import { Routes } from '@angular/router';
import { ProjectListComponent } from './components/project-list/project-list.component';
import { ProjectCreateComponent } from './components/project-create/project-create.component';
import { ProjectDetailComponent } from './components/project-detail/project-detail.component';
import { PlanningIntegratedComponent } from './components/planning/planning-integrated.component';
import { HomeComponent } from './home.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'planning', component: PlanningIntegratedComponent },
  { path: 'planning/:projectId', component: PlanningIntegratedComponent },
  { path: 'projects', component: ProjectListComponent },
  { path: 'projects/new', component: ProjectCreateComponent },
  { path: 'projects/:id', component: ProjectDetailComponent },
  { path: '**', redirectTo: '/' }
];
