import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CyrenaPersistenceCoreComponent } from './cyrena-persistence-core.component';

describe('CyrenaPersistenceCoreComponent', () => {
  let component: CyrenaPersistenceCoreComponent;
  let fixture: ComponentFixture<CyrenaPersistenceCoreComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CyrenaPersistenceCoreComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CyrenaPersistenceCoreComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
