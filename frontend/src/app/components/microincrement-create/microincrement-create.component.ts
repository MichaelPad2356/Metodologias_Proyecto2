import { Component } from '@angular/core';

import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MicroincrementService } from '../../services/microincrement.service';
import { CreateMicroincrementRequest } from '../../models/microincrement.model';
import { MicroincrementListComponent } from '../microincrement-list/microincrement-list.component';

@Component({
  selector: 'app-microincrement-create',
  standalone: true,
  imports: [FormsModule, MicroincrementListComponent],
  templateUrl: './microincrement-create.component.html',
  styleUrls: ['./microincrement-create.component.scss']
})
export class MicroincrementCreateComponent {
  form: CreateMicroincrementRequest = {
    title: '',
    description: '',
    projectPhaseId: 0,
    deliverableId: undefined,
    author: ''
  };

  loading = false;
  error = '';
  success = false;

  constructor(
    private microincrementService: MicroincrementService,
    private router: Router
  ) { }

  onSubmit(): void {
    if (!this.form.title || !this.form.projectPhaseId || !this.form.author) {
      this.error = 'Por favor completa los campos requeridos';
      return;
    }

    this.loading = true;
    this.error = '';

    this.microincrementService.create(this.form).subscribe({
      next: (data) => {
        this.success = true;
        this.loading = false;
        setTimeout(() => {
          this.router.navigate(['/microincrements']);
        }, 1500);
      },
      error: (err) => {
        this.error = 'Error al crear microincremento';
        this.loading = false;
        console.error(err);
      }
    });
  }

  resetForm(): void {
    this.form = {
      title: '',
      description: '',
      projectPhaseId: 0,
      deliverableId: undefined,
      author: ''
    };
    this.error = '';
    this.success = false;
  }
}