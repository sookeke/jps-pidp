import { PartyOrganizationDetails } from '@bcgov/shared/data-access';

import { Section } from '../section.model';

/**
 * @description
 * College certification HTTP response model for a section.
 */
export interface PartyOrganizationDetailsSection
  extends Pick<
      PartyOrganizationDetails,
      | 'organizationCode'
      | 'employeeIdentifier'
      | 'orgName'
      | 'correctionService'
      | 'correctionServiceCode'
      | 'justiceSectorService'
    >,
    Section {}
