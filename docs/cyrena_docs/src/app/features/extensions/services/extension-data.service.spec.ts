import { TestBed } from '@angular/core/testing';

import { ExtensionDataService } from './extension-data.service';

describe('ExtensionDataService', () => {
  let service: ExtensionDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ExtensionDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
