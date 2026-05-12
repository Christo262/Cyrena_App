import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UiOverviewDocComponent } from './ui-overview-doc.component';

describe('UiOverviewDocComponent', () => {
  let component: UiOverviewDocComponent;
  let fixture: ComponentFixture<UiOverviewDocComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UiOverviewDocComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UiOverviewDocComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
