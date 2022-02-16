import { Injectable } from '@angular/core';
import { Resolve, Router } from '@angular/router';

import { Observable, catchError, exhaustMap, of, throwError } from 'rxjs';

import { PartyResource } from '../resources/party-resource.service';
import { LoggerService } from '../services/logger.service';
import { PartyService } from '../services/party.service';

/**
 * @description
 * Gets a Party from the API based on the access token, and if not
 * found creates the resource before setting it in a local service.
 *
 * WARNING: Should be located on or under the route config containing
 * guard(s) that manage redirection.
 */
@Injectable({
  providedIn: 'root',
})
export class PartyResolver implements Resolve<number | null> {
  public constructor(
    private router: Router,
    private partyResource: PartyResource,
    private partyService: PartyService,
    private logger: LoggerService
  ) {}

  public resolve(): Observable<number | null> {
    return this.partyResource.firstOrCreate().pipe(
      exhaustMap((partyId: number | null) =>
        partyId
          ? of((this.partyService.partyId = partyId))
          : throwError(() => new Error('Party could not be found or created'))
      ),
      catchError((error: Error) => {
        this.logger.error(error.message);
        // TODO could redirect to root, but possible to create an infinite loop
        // this.router.navigate(['/']);
        // TODO could redirect to an appropriate error page but what error page?
        // this.router.navigateByUrl(RootRoutes.DENIED);
        return of(null);
      })
    );
  }
}
