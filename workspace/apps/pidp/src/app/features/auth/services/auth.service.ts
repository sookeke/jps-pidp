import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';

import { Observable, catchError, from, map } from 'rxjs';

import { KeycloakService } from 'keycloak-angular';
import { KeycloakLoginOptions } from 'keycloak-js';

import { APP_CONFIG, AppConfig } from '@app/app.config';
import { ApiHttpClient } from '@app/core/resources/api-http-client.service';

import { IdentityProvider } from '../enums/identity-provider.enum';

export interface IAuthService {
  login(options?: KeycloakLoginOptions): Observable<void>;
  isLoggedIn(): Observable<boolean>;
  logout(redirectUri: string, idp: IdentityProvider): Observable<void>;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService implements IAuthService {
  public constructor(
    @Inject(APP_CONFIG) private config: AppConfig,

    private keycloakService: KeycloakService,
    private apiResource: ApiHttpClient,
    private http: HttpClient
  ) {}

  public login(options?: KeycloakLoginOptions): Observable<void> {
    return from(this.keycloakService.login(options));
  }

  public isLoggedIn(): Observable<boolean> {
    return from(this.keycloakService.isLoggedIn());
  }

  public logout(redirectUri: string, idp: IdentityProvider): Observable<void> {
    if (this.config.idpConfig[idp]) {
      return from(this.keycloakService.logout(this.config.idpConfig[idp]));
    } else {
      return from(this.keycloakService.logout(redirectUri));
    }
  }
}
