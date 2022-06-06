import { Injectable } from '@angular/core';
import { Resolve } from '@angular/router';

import { Observable, catchError, map, of } from 'rxjs';

import { PartyService } from '@app/core/party/party.service';
import { StatusCode } from '@app/features/portal/enums/status-code.enum';
import { ProfileStatus } from '@app/features/portal/models/profile-status.model';

import { DigitalEvidenceResource } from './digital-evidence-resource.service';

@Injectable({
  providedIn: 'root',
})
export class DigitalEvidenceResolver implements Resolve<StatusCode | null> {
  public constructor(
    private partyService: PartyService,
    private resource: DigitalEvidenceResource
  ) {}

  public resolve(): Observable<StatusCode | null> {
    if (!this.partyService.partyId) {
      return of(null);
    }

    return this.resource.getProfileStatus(this.partyService.partyId).pipe(
      map((profileStatus: ProfileStatus | null) => {
        if (!profileStatus) {
          return null;
        }

        return profileStatus.status.digitalEvidence.statusCode;
      }),
      catchError(() => of(null))
    );
  }
}
