import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CyrenaComponentsCoreComponent } from './cyrena-components-core.component';

describe('CyrenaComponentsCoreComponent', () => {
  let component: CyrenaComponentsCoreComponent;
  let fixture: ComponentFixture<CyrenaComponentsCoreComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CyrenaComponentsCoreComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CyrenaComponentsCoreComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
