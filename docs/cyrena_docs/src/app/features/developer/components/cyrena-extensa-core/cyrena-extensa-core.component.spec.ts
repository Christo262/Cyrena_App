import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CyrenaExtensaCoreComponent } from './cyrena-extensa-core.component';

describe('CyrenaExtensaCoreComponent', () => {
  let component: CyrenaExtensaCoreComponent;
  let fixture: ComponentFixture<CyrenaExtensaCoreComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CyrenaExtensaCoreComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CyrenaExtensaCoreComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
