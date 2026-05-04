import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExtensionDevelopmentComponent } from './extension-development.component';

describe('ExtensionDevelopmentComponent', () => {
  let component: ExtensionDevelopmentComponent;
  let fixture: ComponentFixture<ExtensionDevelopmentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExtensionDevelopmentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ExtensionDevelopmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
