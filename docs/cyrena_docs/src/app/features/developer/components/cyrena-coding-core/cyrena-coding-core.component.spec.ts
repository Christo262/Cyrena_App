import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CyrenaCodingCoreComponent } from './cyrena-coding-core.component';

describe('CyrenaCodingCoreComponent', () => {
  let component: CyrenaCodingCoreComponent;
  let fixture: ComponentFixture<CyrenaCodingCoreComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CyrenaCodingCoreComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CyrenaCodingCoreComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
