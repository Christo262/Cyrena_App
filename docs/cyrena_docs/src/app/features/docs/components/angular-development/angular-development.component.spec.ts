import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AngularDevelopmentComponent } from './angular-development.component';

describe('AngularDevelopmentComponent', () => {
  let component: AngularDevelopmentComponent;
  let fixture: ComponentFixture<AngularDevelopmentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AngularDevelopmentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AngularDevelopmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
