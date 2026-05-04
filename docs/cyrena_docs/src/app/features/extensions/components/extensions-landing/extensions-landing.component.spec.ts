import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExtensionsLandingComponent } from './extensions-landing.component';

describe('ExtensionsLandingComponent', () => {
  let component: ExtensionsLandingComponent;
  let fixture: ComponentFixture<ExtensionsLandingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExtensionsLandingComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ExtensionsLandingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
