import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface WorkflowStep {
  name: string;
  order?: number;
}

interface Workflow {
  id?: number;
  name: string;
  description: string;
  steps: WorkflowStep[]; // SIN el ? (sin hacer opcional)
}

@Component({
  selector: 'app-workflows',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './workflows.component.html',
  styleUrls: ['./workflows.component.scss']
})
export class WorkflowsComponent implements OnInit {
  workflows: Workflow[] = [];
  showModal = false;
  editingWorkflow: Workflow | null = null;
  formData: Workflow = {
    name: '',
    description: '',
    steps: [{ name: '' }]
  };

  private apiUrl = 'http://localhost:5062/api/workflows';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadWorkflows();
  }

  loadWorkflows() {
    this.http.get<Workflow[]>(this.apiUrl).subscribe({
      next: (data) => this.workflows = data,
      error: (err) => console.error('Error loading workflows:', err)
    });
  }

  openModal(workflow?: Workflow) {
    if (workflow) {
      this.editingWorkflow = workflow;
      this.formData = {
        name: workflow.name,
        description: workflow.description,
        steps: workflow.steps.map(s => ({ name: s.name }))
      };
    } else {
      this.editingWorkflow = null;
      this.formData = {
        name: '',
        description: '',
        steps: [{ name: '' }]
      };
    }
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.editingWorkflow = null;
  }

  onSubmit() {
    const request = this.editingWorkflow
      ? this.http.put(`${this.apiUrl}/${this.editingWorkflow.id}`, this.formData)
      : this.http.post(this.apiUrl, this.formData);

    request.subscribe({
      next: () => {
        this.loadWorkflows();
        this.closeModal();
      },
      error: (err) => console.error('Error saving workflow:', err)
    });
  }

  deleteWorkflow(id: number) {
    if (confirm('¿Estás seguro de eliminar este flujo de trabajo?')) {
      this.http.delete(`${this.apiUrl}/${id}`).subscribe({
        next: () => this.loadWorkflows(),
        error: (err) => console.error('Error deleting workflow:', err)
      });
    }
  }

  addStep() {
    this.formData.steps.push({ name: '' });
  }

  removeStep(index: number) {
    if (this.formData.steps.length > 1) {
      this.formData.steps.splice(index, 1);
    }
  }
}
