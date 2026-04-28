import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ApiReferenceComponent } from './api-reference.component';

describe('ApiReferenceComponent', () => {
  let component: ApiReferenceComponent;
  let fixture: ComponentFixture<ApiReferenceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ApiReferenceComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ApiReferenceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
