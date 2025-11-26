import { Routes } from '@angular/router';
import { ProjectListComponent } from './components/project-list/project-list.component';
import { ProjectCreateComponent } from './components/project-create/project-create.component';
import { ProjectDetailComponent } from './components/project-detail/project-detail.component';
import { IterationManagementComponent } from './components/iteration-management/iteration-management.component';
import { PlanningComponent } from './planning.component';
import { HomeComponent } from './home.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'planning', component: PlanningComponent },
  { path: 'projects', component: ProjectListComponent },
  { path: 'projects/new', component: ProjectCreateComponent },
  { path: 'projects/:id', component: ProjectDetailComponent },
  { path: 'projects/:id/iterations', component: IterationManagementComponent },
  { path: '**', redirectTo: '/' }
];
