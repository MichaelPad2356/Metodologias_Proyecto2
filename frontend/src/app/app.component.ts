import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermissionService, UserRole } from './services/permission.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'Gestión de Proyectos OpenUP';
  currentRole: UserRole = 'Admin';
  roles: UserRole[] = ['Admin', 'ProjectManager', 'Developer', 'Tester', 'Stakeholder'];

  constructor(private permService: PermissionService) {
    this.permService.role$.subscribe(role => this.currentRole = role);
  }

  onRoleChange() {
    this.permService.setRole(this.currentRole);
  }
}
