import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProjectMemberService } from '../../services/project-member.service';
import { ProjectMember, AVAILABLE_ROLES, InviteMemberRequest } from '../../models/project-member.model';
import { PermissionService } from '../../services/permission.service';

@Component({
  selector: 'app-project-members',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './project-members.component.html',
  styleUrls: ['./project-members.component.scss']
})
export class ProjectMembersComponent implements OnInit, OnChanges {
  @Input() projectId!: number;

  members: ProjectMember[] = [];
  loading = false;
  error: string | null = null;
  successMessage: string | null = null;
  
  // Permisos
  canManageMembers: boolean = false;

  // Modal
  showInviteModal = false;
  showEditModal = false;

  // Form data
  inviteForm: InviteMemberRequest = {
    userEmail: '',
    userName: '',
    role: 'Desarrollador'
  };

  editingMember: ProjectMember | null = null;
  selectedRole: string = 'Desarrollador';

  // Role options - roles disponibles desde el modelo
  roleOptions = AVAILABLE_ROLES;

  constructor(
    private memberService: ProjectMemberService,
    private permService: PermissionService
  ) {}

  ngOnInit(): void {
    this.permService.role$.subscribe(() => {
      this.canManageMembers = this.permService.canManageMembers();
    });
    
    if (this.projectId) {
      this.loadMembers();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['projectId'] && this.projectId) {
      this.loadMembers();
    }
  }

  private loadMembers(): void {
    this.loading = true;
    this.memberService.getProjectMembers(this.projectId).subscribe({
      next: (data) => {
        this.members = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading members:', err);
        this.loading = false;
      }
    });
  }

  // ==================== INVITE ====================

  openInviteModal(): void {
    this.inviteForm = {
      userEmail: '',
      userName: '',
      role: 'Desarrollador'
    };
    this.showInviteModal = true;
  }

  closeInviteModal(): void {
    this.showInviteModal = false;
  }

  inviteMember(): void {
    if (!this.inviteForm.userEmail.trim()) {
      this.showError('Email es requerido');
      return;
    }

    this.memberService.inviteMember(this.projectId, this.inviteForm).subscribe({
      next: () => {
        this.showSuccess('Miembro invitado exitosamente');
        this.loadMembers();
        this.closeInviteModal();
      },
      error: (err) => this.showError(err.error?.message || err.error || 'Error al invitar miembro')
    });
  }

  // ==================== EDIT ROLE ====================

  openEditModal(member: ProjectMember): void {
    this.editingMember = { ...member };
    this.selectedRole = member.role;
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.editingMember = null;
  }

  updateMemberRole(): void {
    if (!this.editingMember) return;

    this.memberService.updateMemberRole(this.projectId, this.editingMember.id, {
      role: this.selectedRole
    }).subscribe({
      next: () => {
        this.showSuccess('Rol actualizado exitosamente');
        this.loadMembers();
        this.closeEditModal();
      },
      error: (err) => this.showError(err.error?.message || 'Error al actualizar rol')
    });
  }

  // ==================== REMOVE ====================

  removeMember(member: ProjectMember): void {
    const name = member.userName || member.userEmail;
    if (!confirm(`¿Está seguro de eliminar a "${name}" del proyecto?`)) return;

    this.memberService.removeMember(this.projectId, member.id).subscribe({
      next: () => {
        this.showSuccess('Miembro eliminado del proyecto');
        this.loadMembers();
      },
      error: (err) => this.showError(err.error?.message || 'Error al eliminar miembro')
    });
  }

  // ==================== ACCEPT INVITATION ====================

  acceptInvitation(member: ProjectMember): void {
    this.memberService.acceptInvitation(this.projectId, member.id).subscribe({
      next: () => {
        this.showSuccess('Invitación aceptada');
        this.loadMembers();
      },
      error: (err) => this.showError(err.error?.message || 'Error al aceptar invitación')
    });
  }

  // ==================== HELPERS ====================

  getRoleClass(role: string): string {
    const classMap: Record<string, string> = {
      'Administrador': 'role-admin',
      'Product Owner': 'role-po',
      'Scrum Master': 'role-sm',
      'Desarrollador': 'role-dev',
      'Tester': 'role-tester',
      'Autor': 'role-author',
      'Revisor': 'role-reviewer'
    };
    return classMap[role] || 'role-default';
  }

  getStatusClass(status: string): string {
    const classMap: Record<string, string> = {
      'Pending': 'status-pending',
      'Accepted': 'status-active',
      'Declined': 'status-declined',
      'Removed': 'status-removed'
    };
    return classMap[status] || '';
  }

  getStatusLabel(status: string): string {
    const labelMap: Record<string, string> = {
      'Pending': 'Pendiente',
      'Accepted': 'Aceptada',
      'Declined': 'Rechazada',
      'Removed': 'Removido'
    };
    return labelMap[status] || status;
  }

  private showSuccess(message: string): void {
    this.successMessage = message;
    this.error = null;
    setTimeout(() => this.successMessage = null, 3000);
  }

  private showError(message: string): void {
    this.error = message;
    this.successMessage = null;
    setTimeout(() => this.error = null, 5000);
  }

  formatDate(dateString: string | undefined): string {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
