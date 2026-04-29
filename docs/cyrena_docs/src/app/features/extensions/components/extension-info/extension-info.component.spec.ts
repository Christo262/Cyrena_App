import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExtensionInfoComponent } from './extension-info.component';

describe('ExtensionInfoComponent', () => {
  let component: ExtensionInfoComponent;
  let fixture: ComponentFixture<ExtensionInfoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExtensionInfoComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ExtensionInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
