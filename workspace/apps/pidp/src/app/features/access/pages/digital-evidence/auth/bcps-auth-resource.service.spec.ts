import { TestBed } from '@angular/core/testing';

import { BcpsAuthResourceService } from './bcps-auth-resource.service';

describe('BcpsAuthResourceService', () => {
  let service: BcpsAuthResourceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BcpsAuthResourceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
