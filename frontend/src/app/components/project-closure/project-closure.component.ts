import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClosureService } from '../../services/closure.service';
import { ClosureValidation, ClosureResult } from '../../models/closure.model';
import { PermissionService } from '../../services/permission.service';

@Component({
  selector: 'app-project-closure',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './project-closure.component.html',
  styleUrls: ['./project-closure.component.scss']
})
export class ProjectClosureComponent implements OnInit {
  @Input() projectId!: number;
  @Input() projectName: string = '';
  @Output() projectClosed = new EventEmitter<void>();

  validation: ClosureValidation | null = null;
  loading = false;
  closing = false;
  error: string | null = null;
  successMessage: string | null = null;
  
  // Permisos
  canClose: boolean = false;

  // Modal
  showModal = false;

  // Form
  closedBy: string = '';
  closureNotes: string = '';
  forceJustification: string = '';

  constructor(
    private closureService: ClosureService,
    private permService: PermissionService
  ) {}

  ngOnInit(): void {
    this.permService.role$.subscribe(() => {
      this.canClose = this.permService.canCloseProject();
    });
  }

  openClosureModal(): void {
    if (!this.canClose) {
      this.error = this.permService.getPermissionDeniedMessage('cerrar el proyecto');
      return;
    }
    this.showModal = true;
    this.error = null;
    this.successMessage = null;
    this.validateClosure();
  }

  closeModal(): void {
    this.showModal = false;
    this.validation = null;
  }

  private validateClosure(): void {
    this.loading = true;
    this.closureService.validateClosure(this.projectId).subscribe({
      next: (result) => {
        this.validation = result;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al validar cierre';
        this.loading = false;
      }
    });
  }

  closeProject(): void {
    if (!this.closedBy.trim()) {
      this.error = 'Ingrese su nombre';
      return;
    }

    this.closing = true;
    this.closureService.closeProject(this.projectId, {
      closedBy: this.closedBy,
      closureNotes: this.closureNotes
    }).subscribe({
      next: (result) => this.handleClosureResult(result),
      error: (err) => {
        this.error = err.error?.message || 'Error al cerrar proyecto';
        this.closing = false;
      }
    });
  }

  forceCloseProject(): void {
    if (!this.closedBy.trim()) {
      this.error = 'Ingrese su nombre';
      return;
    }
    if (!this.forceJustification.trim()) {
      this.error = 'Debe proporcionar una justificación para el cierre forzado';
      return;
    }

    if (!confirm('¿Está seguro de forzar el cierre? Esta acción no se puede deshacer.')) {
      return;
    }

    this.closing = true;
    this.closureService.forceCloseProject(this.projectId, {
      closedBy: this.closedBy,
      justification: this.forceJustification
    }).subscribe({
      next: (result) => this.handleClosureResult(result),
      error: (err) => {
        this.error = err.error?.message || 'Error al forzar cierre';
        this.closing = false;
      }
    });
  }

  private handleClosureResult(result: ClosureResult): void {
    this.closing = false;
    if (result.success) {
      this.successMessage = result.message;
      setTimeout(() => {
        this.closeModal();
        this.projectClosed.emit();
      }, 2000);
    } else {
      this.error = result.message;
    }
  }

  getCheckIcon(passed: boolean): string {
    return passed ? '✅' : '❌';
  }

  getOverallStatus(): 'ready' | 'warning' | 'blocked' {
    if (!this.validation) return 'blocked';
    if (this.validation.isValid) return 'ready';
    
    const failedChecks = this.validation.checks.filter(c => !c.passed).length;
    return failedChecks <= 2 ? 'warning' : 'blocked';
  }
}
