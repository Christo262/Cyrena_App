import { TestBed } from '@angular/core/testing';

import { ModelReviewsServiceService } from './model-reviews-service.service';

describe('ModelReviewsServiceService', () => {
  let service: ModelReviewsServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ModelReviewsServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
