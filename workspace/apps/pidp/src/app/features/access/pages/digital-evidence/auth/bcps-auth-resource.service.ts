import {
  HttpErrorResponse,
  HttpResponse,
  HttpStatusCode,
} from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, catchError, map, of, throwError } from 'rxjs';

import { CrudResource } from '@bcgov/shared/data-access';

import { ApiHttpClient } from '@app/core/resources/api-http-client.service';

import { AssignedRegion } from '../digital-evidence-account.model';

@Injectable({
  providedIn: 'root',
})
export class BcpsAuthResourceService extends CrudResource<AssignedRegion> {
  protected getResourcePath(id: number): string {
    throw new Error('Method not implemented.');
  }
  public constructor(protected apiResource: ApiHttpClient) {
    super(apiResource);
  }
  public getUserOrgUnit(
    partyId: number,
    participantId: number
  ): Observable<AssignedRegion[]> {
    return this.apiResource
      .get<AssignedRegion[]>(`parties/${partyId}/crown-region/${participantId}/user-orgunit`)
      .pipe(
        map((data: AssignedRegion[]) => {
          return data;
        }),
        catchError((error: HttpErrorResponse) => {
          if (error.status == HttpStatusCode.BadRequest) {
            return of([]);
          }
          return throwError(() => error);
        })
      );
  }
}
