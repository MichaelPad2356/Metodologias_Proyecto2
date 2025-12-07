import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { WorkflowsComponent } from './workflows.component';

describe('WorkflowsComponent', () => {
  let component: WorkflowsComponent;
  let fixture: ComponentFixture<WorkflowsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WorkflowsComponent, HttpClientTestingModule]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WorkflowsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
