import { KeycloakOptions } from 'keycloak-angular';

import { environmentName } from './environment.model';

export interface EnvironmentConfig {
  apiEndpoint: string;
  authEndpoint: string;
  authRealm: string;
  environmentName: environmentName;
  applicationUrl: string;
  keycloakConfig: KeycloakOptions;
}
