import { Injectable } from '@angular/core';

import { AccessTokenService } from '@app/features/auth/services/access-token.service';

@Injectable({
  providedIn: 'root',
})
export class PermissionsService {
  public constructor(private accessTokenService: AccessTokenService) {}

  public hasRole(allowedRoles: string | string[]): boolean {
    allowedRoles = Array.isArray(allowedRoles) ? allowedRoles : [allowedRoles];
    return this.accessTokenService
      .roles()
      .some((role) => allowedRoles.includes(role));
  }

  public hasGroup(allowedGroups: string | string[]): boolean {
    allowedGroups = Array.isArray(allowedGroups)
      ? allowedGroups
      : [allowedGroups];
    return this.accessTokenService
      .groups()
      .some((group) => allowedGroups.includes(group));
  }
}
