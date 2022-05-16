import { Section } from '../section.model';

/**
 * @description
 * Section keys as a readonly tuple to allow iteration
 * over keys at runtime to allow filtering or grouping
 * sections.
 */
export const trainingSectionKeys = ['complianceTraining'] as const;

/**
 * @description
 * Union of keys generated from the tuple.
 */
export type TrainingSectionKey = typeof trainingSectionKeys[number];

/**
 * @description
 * Typing for a group generated from a union.
 */
export type ITrainingGroup = {
  [K in TrainingSectionKey]: Section;
};

/**
 * @description
 * Type used to ensure adding a new key to the tuple is
 * included in the group interface.
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
type CheckGroup<T extends ITrainingGroup = TrainingGroup> = void;

export interface TrainingGroup {
  complianceTraining: Section;
}