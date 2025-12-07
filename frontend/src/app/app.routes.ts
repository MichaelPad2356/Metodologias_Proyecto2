import { Routes } from '@angular/router';
import { ProjectListComponent } from './components/project-list/project-list.component';
import { ProjectCreateComponent } from './components/project-create/project-create.component';
import { ProjectDetailComponent } from './components/project-detail/project-detail.component';
import { PlanningIntegratedComponent } from './components/planning/planning-integrated.component';
import { HomeComponent } from './home.component';
import { MicroincrementListComponent } from './components/microincrement-list/microincrement-list.component';
import { MicroincrementCreateComponent } from './components/microincrement-create/microincrement-create.component';
import { TransitionArtifactsComponent } from './components/transition-artifacts/transition-artifacts.component';
import { DefectListComponent } from './components/defect-list/defect-list.component';
import { DefectCreateComponent } from './components/defect-create/defect-create.component';
import { WorkflowsComponent } from './components/workflows/workflows.component';
import { ConfigurationComponent } from './components/configuration/configuration.component';
import { TemplatesComponent } from './components/templates/templates.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { InvitationsComponent } from './components/invitations/invitations.component';
import { authGuard, guestGuard } from './guards/auth.guard';

export const routes: Routes = [
  // Rutas públicas (solo para usuarios NO autenticados)
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },
  
  // Rutas protegidas (requieren autenticación)
  { path: '', component: HomeComponent, canActivate: [authGuard] },
  { path: 'invitations', component: InvitationsComponent, canActivate: [authGuard] },
  { path: 'planning', component: PlanningIntegratedComponent, canActivate: [authGuard] },
  { path: 'planning/:projectId', component: PlanningIntegratedComponent, canActivate: [authGuard] },
  { path: 'projects', component: ProjectListComponent, canActivate: [authGuard] },
  { path: 'projects/new', component: ProjectCreateComponent, canActivate: [authGuard] },
  { path: 'projects/:id', component: ProjectDetailComponent, canActivate: [authGuard] },
  { path: 'projects/:id/transition', component: TransitionArtifactsComponent, canActivate: [authGuard] },
  { path: 'microincrements', component: MicroincrementListComponent, canActivate: [authGuard] },
  { path: 'microincrements/new', component: MicroincrementCreateComponent, canActivate: [authGuard] },
  { path: 'defects', component: DefectListComponent, canActivate: [authGuard] },
  { path: 'defects/new', component: DefectCreateComponent, canActivate: [authGuard] },
  { path: 'workflows', component: WorkflowsComponent, canActivate: [authGuard] },
  { path: 'configuration', component: ConfigurationComponent, canActivate: [authGuard] },
  { path: 'templates', component: TemplatesComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/' }
];
