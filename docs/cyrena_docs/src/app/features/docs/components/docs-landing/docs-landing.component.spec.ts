import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocsLandingComponent } from './docs-landing.component';

describe('DocsLandingComponent', () => {
  let component: DocsLandingComponent;
  let fixture: ComponentFixture<DocsLandingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DocsLandingComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DocsLandingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
