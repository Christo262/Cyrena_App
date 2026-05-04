import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlatformioDevelopmentComponent } from './platformio-development.component';

describe('PlatformioDevelopmentComponent', () => {
  let component: PlatformioDevelopmentComponent;
  let fixture: ComponentFixture<PlatformioDevelopmentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlatformioDevelopmentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlatformioDevelopmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
