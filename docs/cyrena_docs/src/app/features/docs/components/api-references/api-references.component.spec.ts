import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ApiReferencesComponent } from './api-references.component';

describe('ApiReferencesComponent', () => {
  let component: ApiReferencesComponent;
  let fixture: ComponentFixture<ApiReferencesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ApiReferencesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ApiReferencesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
