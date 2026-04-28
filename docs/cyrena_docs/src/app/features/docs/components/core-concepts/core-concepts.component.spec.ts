import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CoreConceptsComponent } from './core-concepts.component';

describe('CoreConceptsComponent', () => {
  let component: CoreConceptsComponent;
  let fixture: ComponentFixture<CoreConceptsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CoreConceptsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CoreConceptsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
