import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { InvitationService } from './services/invitation.service';
import { User } from './models/auth.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'Gestión de Proyectos OpenUP';
  currentUser: User | null = null;
  isAuthenticated = false;
  pendingInvitationsCount = 0;

  constructor(
    private authService: AuthService,
    private invitationService: InvitationService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      this.isAuthenticated = !!user;
      
      // Cargar invitaciones pendientes si está autenticado
      if (user) {
        this.loadPendingInvitations();
      }
    });

    // Suscribirse a cambios en el contador
    this.invitationService.pendingCount$.subscribe(count => {
      this.pendingInvitationsCount = count;
    });
  }

  loadPendingInvitations(): void {
    this.invitationService.getPendingCount().subscribe({
      error: () => this.pendingInvitationsCount = 0
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
