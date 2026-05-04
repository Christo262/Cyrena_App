import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocLayoutComponent } from './doc-layout.component';

describe('DocLayoutComponent', () => {
  let component: DocLayoutComponent;
  let fixture: ComponentFixture<DocLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DocLayoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DocLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
