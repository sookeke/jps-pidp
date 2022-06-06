import { digitalEvidenceSupportEmail } from '@app/features/access/pages/digital-evidence/digital-evidence.constants';
import { driverFitnessSupportEmail } from '@app/features/access/pages/driver-fitness/driver-fitness.constants';
import { hcimWebAccountTransferSupport } from '@app/features/access/pages/hcim-account-transfer/hcim-account-transfer-constants';
import { hcimWebEnrolmentSupport } from '@app/features/access/pages/hcim-enrolment/hcim-enrolment-constants';
import {
  specialAuthorityEformsSupportEmail,
  specialAuthorityUrl,
} from '@app/features/access/pages/sa-eforms/sa-eforms.constants';

import { AppEnvironment, EnvironmentName } from './environment.model';

/**
 * @description
 * Production environment populated with the default
 * environment information and appropriate overrides.
 *
 * NOTE: This environment is for local development from
 * within a container, and not used within the deployment
 * pipeline. For pipeline config mapping see main.ts and
 * the AppConfigModule.
 */
export const environment: AppEnvironment = {
  production: true,
  apiEndpoint: 'http://localhost:5050',
  environmentName: EnvironmentName.LOCAL,
  applicationUrl: 'http://localhost:4200',
  emails: {
    providerIdentitySupport: 'jpsprovideridentityportal@gov.bc.ca',
    specialAuthorityEformsSupport: specialAuthorityEformsSupportEmail,
    hcimAccountTransferSupport: hcimWebAccountTransferSupport,
    hcimEnrolmentSupport: hcimWebEnrolmentSupport,
    driverFitnessSupport: driverFitnessSupportEmail,
    digitalEvidenceSupport: digitalEvidenceSupportEmail,
  },
  urls: {
    bcscSupport: `https://www2.gov.bc.ca/gov/content/governments/government-id/bcservicescardapp/help`,
    bcscMobileSetup: 'https://id.gov.bc.ca/account',
    specialAuthority: specialAuthorityUrl,
  },
  keycloakConfig: {
    config: {
      url: 'https://sso-dev-5b7aa5-dev.apps.silver.devops.gov.bc.ca/auth',
      realm: 'DEMSPOC',
      clientId: 'PIDP-WEBAPP',
    },
    initOptions: {
      onLoad: 'check-sso',
    },
  },
};
