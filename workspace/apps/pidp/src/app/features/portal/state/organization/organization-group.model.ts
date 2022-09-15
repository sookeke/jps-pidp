import { OrganizationDetails } from '@app/features/organization-info/pages/organization-details/organization-details.model';

import { Section } from '../section.model';
import { AdministratorInfoSection } from './administrator-information-section.model';
import { PartyOrganizationDetailsSection } from './organization-details-section.model';

/**
 * @description
 * Section keys as a readonly tuple to allow iteration
 * over keys at runtime to allow filtering or grouping
 * sections.
 */
export const organizationSectionKeys = [
  'organizationDetails',
  'facilityDetails',
  'administratorInfo',
  'endorsements',
] as const;

/**
 * @description
 * Union of keys generated from the tuple.
 */
export type OrganizationSectionKey = typeof organizationSectionKeys[number];

/**
 * @description
 * Typing for a group generated from a union.
 */
export type IOrganizationGroup = {
  [K in OrganizationSectionKey]: Section;
};

export interface OrganizationGroup extends IOrganizationGroup {
  organizationDetails: PartyOrganizationDetailsSection;
  facilityDetails: Section;
  administratorInfo: AdministratorInfoSection;
  endorsements: Section;
}
