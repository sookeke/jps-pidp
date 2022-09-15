import { TestBed } from '@angular/core/testing';

import { UsertypeResourceService } from './usertype-resource.service';

describe('UsertypeResourceService', () => {
  let service: UsertypeResourceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UsertypeResourceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
