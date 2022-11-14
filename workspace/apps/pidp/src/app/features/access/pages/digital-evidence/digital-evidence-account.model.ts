export interface DemsAccount {
  organizationType: string;
  organizationName: string;
  participantId: string;
  assignedRegion: AssignedRegion[];
}
export interface AssignedRegion {
  regionId: number;
  regionName: string;
  assignedAgency: string;
}
