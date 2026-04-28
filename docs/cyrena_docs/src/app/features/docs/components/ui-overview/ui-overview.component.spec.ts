import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UiOverviewComponent } from './ui-overview.component';

describe('UiOverviewComponent', () => {
  let component: UiOverviewComponent;
  let fixture: ComponentFixture<UiOverviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UiOverviewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UiOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
