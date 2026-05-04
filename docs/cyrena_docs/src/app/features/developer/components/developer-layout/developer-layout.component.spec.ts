import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeveloperLayoutComponent } from './developer-layout.component';

describe('DeveloperLayoutComponent', () => {
  let component: DeveloperLayoutComponent;
  let fixture: ComponentFixture<DeveloperLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeveloperLayoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeveloperLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
