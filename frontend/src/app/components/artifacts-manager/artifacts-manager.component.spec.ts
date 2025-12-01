import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { ArtifactsManagerComponent } from './artifacts-manager.component';

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
});
