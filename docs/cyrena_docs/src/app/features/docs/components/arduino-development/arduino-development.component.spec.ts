import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ArduinoDevelopmentComponent } from './arduino-development.component';

describe('ArduinoDevelopmentComponent', () => {
  let component: ArduinoDevelopmentComponent;
  let fixture: ComponentFixture<ArduinoDevelopmentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ArduinoDevelopmentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ArduinoDevelopmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
