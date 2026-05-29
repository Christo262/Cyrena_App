import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CyrenaVoiceCoreComponent } from './cyrena-voice-core.component';

describe('CyrenaVoiceCoreComponent', () => {
  let component: CyrenaVoiceCoreComponent;
  let fixture: ComponentFixture<CyrenaVoiceCoreComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CyrenaVoiceCoreComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CyrenaVoiceCoreComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
