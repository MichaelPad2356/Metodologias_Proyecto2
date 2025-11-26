import { Routes } from '@angular/router';
import { ProjectListComponent } from './components/project-list/project-list.component';
import { ProjectCreateComponent } from './components/project-create/project-create.component';
import { ProjectDetailComponent } from './components/project-detail/project-detail.component';
import { MicroincrementListComponent } from './components/microincrement-list/microincrement-list.component';
import { MicroincrementCreateComponent } from './components/microincrement-create/microincrement-create.component';

export const routes: Routes = [
  { path: '', redirectTo: '/projects', pathMatch: 'full' },
  { path: 'projects', component: ProjectListComponent },
  { path: 'projects/new', component: ProjectCreateComponent },
  { path: 'projects/:id', component: ProjectDetailComponent },
  { path: 'microincrements', component: MicroincrementListComponent },
  { path: 'microincrements/new', component: MicroincrementCreateComponent },
  { path: '**', redirectTo: '/projects' }
];
