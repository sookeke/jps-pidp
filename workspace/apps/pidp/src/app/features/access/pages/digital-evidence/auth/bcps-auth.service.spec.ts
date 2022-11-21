import { TestBed } from '@angular/core/testing';

import { BcpsAuthService } from './bcps-auth.service';

describe('BcpsAuthService', () => {
  let service: BcpsAuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BcpsAuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
