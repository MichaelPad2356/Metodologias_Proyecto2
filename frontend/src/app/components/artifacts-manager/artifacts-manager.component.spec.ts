import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ArtifactsManagerComponent } from './artifacts-manager.component';
import { ArtifactType } from '../../models/artifact.model';

describe('ArtifactsManagerComponent', () => {
  let component: ArtifactsManagerComponent;
  let fixture: ComponentFixture<ArtifactsManagerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ArtifactsManagerComponent, HttpClientTestingModule]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ArtifactsManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with default values', () => {
    expect(component.artifactForm).toBeDefined();
    expect(component.artifactForm.get('isMandatory')?.value).toBe(false);
    expect(component.artifactForm.get('type')?.value).toBe('');
    expect(component.artifactForm.get('author')?.value).toBe('');
  });

  it('should be invalid when empty', () => {
    expect(component.artifactForm.valid).toBeFalsy();
  });

  it('should be valid when required fields are filled', () => {
    component.artifactForm.patchValue({
      type: ArtifactType.UserManual,
      author: 'Test Author'
    });
    expect(component.artifactForm.valid).toBeTruthy();
  });

  it('should not require name or description', () => {
    // These fields were removed, so we shouldn't need them for validity
    component.artifactForm.patchValue({
      type: ArtifactType.TechnicalManual,
      author: 'Another Author'
    });
    expect(component.artifactForm.valid).toBeTruthy();
  });
});
