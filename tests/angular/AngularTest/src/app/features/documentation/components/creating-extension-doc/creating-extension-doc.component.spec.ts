import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreatingExtensionDocComponent } from './creating-extension-doc.component';

describe('CreatingExtensionDocComponent', () => {
  let component: CreatingExtensionDocComponent;
  let fixture: ComponentFixture<CreatingExtensionDocComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreatingExtensionDocComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreatingExtensionDocComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
