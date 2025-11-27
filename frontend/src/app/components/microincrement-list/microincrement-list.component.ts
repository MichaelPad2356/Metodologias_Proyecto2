import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MicroincrementService } from '../../services/microincrement.service';
import { Microincrement } from '../../models/microincrement.model';

@Component({
  selector: 'app-microincrement-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './microincrement-list.component.html',
  styleUrls: ['./microincrement-list.component.scss']
})
export class MicroincrementListComponent implements OnInit {
  microincrements: Microincrement[] = [];
  filteredMicroincrements: Microincrement[] = [];
  loading = false;
  error = '';

  filterPhaseId: number | null = null;
  filterDeliverableId: number | null = null;
  filterAuthor = '';

  constructor(private microincrementService: MicroincrementService) { }

  ngOnInit(): void {
    this.loadMicroincrements();
  }

  loadMicroincrements(): void {
    this.loading = true;
    this.microincrementService.getAll().subscribe({
      next: (data) => {
        this.microincrements = data;
        this.applyFilters();
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar microincrementos';
        console.error(err);
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.filteredMicroincrements = this.microincrements.filter(m => {
      const matchPhase = !this.filterPhaseId || m.projectPhaseId === this.filterPhaseId;
      const matchDeliverable = !this.filterDeliverableId || m.deliverableId === this.filterDeliverableId;
      const matchAuthor = !this.filterAuthor || m.author.toLowerCase().includes(this.filterAuthor.toLowerCase());
      return matchPhase && matchDeliverable && matchAuthor;
    });
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  deleteMicroincrement(id: number): void {
    if (confirm('¿Está seguro de que desea eliminar este microincremento?')) {
      this.microincrementService.delete(id).subscribe({
        next: () => {
          this.loadMicroincrements();
        },
        error: (err) => {
          this.error = 'Error al eliminar microincremento';
          console.error(err);
        }
      });
    }
  }
}