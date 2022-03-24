import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, catchError, of, throwError } from 'rxjs';

import { NoContent, NoContentResponse } from '@bcgov/shared/data-access';

import { ApiHttpClient } from '@app/core/resources/api-http-client.service';
// TODO refactor to drop dependency between modules unless from core, shared, or lib
// @see the comment in the resolver to push this into PartyService
import { PortalResource } from '@app/features/portal/portal-resource.service';
import { ProfileStatus } from '@app/features/portal/sections/models/profile-status.model';

@Injectable({
  providedIn: 'root',
})
export class SaEformsResource {
  public constructor(
    private apiResource: ApiHttpClient,
    private portalResource: PortalResource
  ) {}

  public getProfileStatus(partyId: number): Observable<ProfileStatus | null> {
    return this.portalResource.getProfileStatus(partyId);
  }

  public requestAccess(partyId: number): NoContent {
    return this.apiResource
      .post<NoContent>('access-requests/sa-eforms', { partyId })
      .pipe(
        NoContentResponse,
        catchError((error: HttpErrorResponse) => {
          if (error.status === HttpStatusCode.BadRequest) {
            return of(void 0);
          }

          return throwError(() => error);
        })
      );
  }
}