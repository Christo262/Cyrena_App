import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocsLandingPageComponent } from './docs-landing-page.component';

describe('DocsLandingPageComponent', () => {
  let component: DocsLandingPageComponent;
  let fixture: ComponentFixture<DocsLandingPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DocsLandingPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DocsLandingPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
