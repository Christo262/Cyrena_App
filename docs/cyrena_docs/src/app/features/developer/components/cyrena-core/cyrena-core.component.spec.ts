import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CyrenaCoreComponent } from './cyrena-core.component';

describe('CyrenaCoreComponent', () => {
  let component: CyrenaCoreComponent;
  let fixture: ComponentFixture<CyrenaCoreComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CyrenaCoreComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CyrenaCoreComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
