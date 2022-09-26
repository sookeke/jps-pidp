import {
  HttpErrorResponse,
  HttpResponse,
  HttpStatusCode,
} from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, catchError, map, of, throwError } from 'rxjs';

import { CrudResource } from '@bcgov/shared/data-access';

import { ApiHttpClient } from '@app/core/resources/api-http-client.service';

import { OrganizationUserType } from './usertype-service.model';

@Injectable({
  providedIn: 'root',
})
export class PartyUserTypeResource extends CrudResource<OrganizationUserType> {
  public constructor(protected apiResource: ApiHttpClient) {
    super(apiResource);
  }

  public getUserType(partyId: number): Observable<OrganizationUserType[]> {
    return this.apiResource
      .get<OrganizationUserType[]>(this.getResourcePath(partyId))
      .pipe(
        map((data: OrganizationUserType[]) => {
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

  protected getResourcePath(partyId: number): string {
    return `parties/${partyId}/user-type`;
  }
}
