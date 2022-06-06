import { Section } from '../section.model';

/**
 * @description
 * Section keys as a readonly tuple to allow iteration
 * over keys at runtime to allow filtering or grouping
 * sections.
 */
export const adminPanelSectionKeys = ['administrationPanel'] as const;

/**
 * @description
 * Union of keys generated from the tuple.
 */
export type AdminPanelSectionKey = typeof adminPanelSectionKeys[number];

/**
 * @description
 * Typing for a group generated from a union.
 */
export type IAdminGroup = {
  [K in AdminPanelSectionKey]: Section;
};

export interface AdminGroup extends IAdminGroup {
  adminPanel: Section;
}
