import { TestBed } from '@angular/core/testing';

import { BcpsAuthResolver } from './bcps-auth.resolver';

describe('BcpsAuthResolver', () => {
  let resolver: BcpsAuthResolver;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    resolver = TestBed.inject(BcpsAuthResolver);
  });

  it('should be created', () => {
    expect(resolver).toBeTruthy();
  });
});
