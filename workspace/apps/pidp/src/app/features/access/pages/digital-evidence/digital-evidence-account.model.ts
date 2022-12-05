export interface DemsAccount {
  organizationType: string;
  organizationName: string;
  participantId: string;
  assignedRegions: AssignedRegion[];
}
export interface AssignedRegion {
  regionId: number;
  regionName: string;
  assignedAgency: string;
}
