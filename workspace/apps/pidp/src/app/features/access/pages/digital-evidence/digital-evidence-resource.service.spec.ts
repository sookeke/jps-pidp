import { TestBed } from '@angular/core/testing';

import { provideAutoSpy } from 'jest-auto-spies';

import { ApiHttpClient } from '@app/core/resources/api-http-client.service';
import { PortalResource } from '@app/features/portal/portal-resource.service';

import { DigitalEvidenceResource } from './digital-evidence-resource.service';

describe('DigitalEvidenceResource', () => {
  let service: DigitalEvidenceResource;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DigitalEvidenceResource,
        provideAutoSpy(ApiHttpClient),
        provideAutoSpy(PortalResource),
      ],
    });
    service = TestBed.inject(DigitalEvidenceResource);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
