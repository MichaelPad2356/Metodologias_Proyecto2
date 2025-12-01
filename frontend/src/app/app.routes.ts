import { Routes } from '@angular/router';
import { ProjectListComponent } from './components/project-list/project-list.component';
import { ProjectCreateComponent } from './components/project-create/project-create.component';
import { ProjectDetailComponent } from './components/project-detail/project-detail.component';
import { PlanningIntegratedComponent } from './components/planning/planning-integrated.component';
import { HomeComponent } from './home.component';
import { MicroincrementListComponent } from './components/microincrement-list/microincrement-list.component';
import { MicroincrementCreateComponent } from './components/microincrement-create/microincrement-create.component';
<<<<<<< HEAD
import { TransitionArtifactsComponent } from './components/transition-artifacts/transition-artifacts.component';
=======
import { DefectListComponent } from './components/defect-list/defect-list.component';
import { DefectCreateComponent } from './components/defect-create/defect-create.component';
>>>>>>> origin/develop

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'planning', component: PlanningIntegratedComponent },
  { path: 'planning/:projectId', component: PlanningIntegratedComponent },
  { path: 'projects', component: ProjectListComponent },
  { path: 'projects/new', component: ProjectCreateComponent },
  { path: 'projects/:id', component: ProjectDetailComponent },
  { path: 'projects/:id/transition', component: TransitionArtifactsComponent },
  { path: 'microincrements', component: MicroincrementListComponent },
  { path: 'microincrements/new', component: MicroincrementCreateComponent },
  { path: 'defects', component: DefectListComponent },
  { path: 'defects/new', component: DefectCreateComponent },
  { path: '**', redirectTo: '/' }
];
