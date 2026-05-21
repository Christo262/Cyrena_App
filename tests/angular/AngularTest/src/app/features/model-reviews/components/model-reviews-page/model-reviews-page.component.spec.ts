import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModelReviewsPageComponent } from './model-reviews-page.component';

describe('ModelReviewsPageComponent', () => {
  let component: ModelReviewsPageComponent;
  let fixture: ComponentFixture<ModelReviewsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModelReviewsPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModelReviewsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
