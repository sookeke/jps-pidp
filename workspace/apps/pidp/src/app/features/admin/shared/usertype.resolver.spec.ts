import { TestBed } from '@angular/core/testing';

import { UsertypeResolver } from './usertype.resolver';

describe('UsertypeResolver', () => {
  let resolver: UsertypeResolver;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    resolver = TestBed.inject(UsertypeResolver);
  });

  it('should be created', () => {
    expect(resolver).toBeTruthy();
  });
});
