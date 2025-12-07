import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { TransitionArtifactsComponent } from './transition-artifacts.component';
import { ArtifactService } from '../../services/artifactService';
import { of } from 'rxjs';
import { Artifact, ArtifactStatus, ArtifactType } from '../../models/artifact.model';

describe('TransitionArtifactsComponent', () => {
  let component: TransitionArtifactsComponent;
  let fixture: ComponentFixture<TransitionArtifactsComponent>;
  let artifactServiceSpy: jasmine.SpyObj<ArtifactService>;

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('ArtifactService', ['getTransitionArtifacts', 'validateProjectClosure']);

    await TestBed.configureTestingModule({
      imports: [TransitionArtifactsComponent, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: ArtifactService, useValue: spy }
      ]
    })
    .compileComponents();

    artifactServiceSpy = TestBed.inject(ArtifactService) as jasmine.SpyObj<ArtifactService>;
    fixture = TestBed.createComponent(TransitionArtifactsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should count pending approval artifacts correctly', () => {
    component.artifacts = [
      { id: 1, type: ArtifactType.UserManual, status: ArtifactStatus.Pending } as Artifact,
      { id: 2, type: ArtifactType.TechnicalManual, status: ArtifactStatus.InReview } as Artifact,
      { id: 3, type: ArtifactType.DeploymentPlan, status: ArtifactStatus.Approved } as Artifact
    ];

    expect(component.getPendingApprovalCount()).toBe(2);
  });

  it('should validate closure checklist correctly', () => {
    // Case 1: No closure doc
    component.artifacts = [];
    expect(component.hasClosureChecklist()).toBeFalse();

    // Case 2: Closure doc with incomplete checklist
    const incompleteChecklist = JSON.stringify([{ description: 'Item 1', isMandatory: true, isCompleted: false }]);
    component.artifacts = [
      { id: 1, type: ArtifactType.ClosureDoc, closureChecklistJson: incompleteChecklist } as Artifact
    ];
    expect(component.hasClosureChecklist()).toBeFalse();

    // Case 3: Closure doc with complete checklist
    const completeChecklist = JSON.stringify([{ description: 'Item 1', isMandatory: true, isCompleted: true }]);
    component.artifacts = [
      { id: 1, type: ArtifactType.ClosureDoc, closureChecklistJson: completeChecklist } as Artifact
    ];
    expect(component.hasClosureChecklist()).toBeTrue();
  });
});
