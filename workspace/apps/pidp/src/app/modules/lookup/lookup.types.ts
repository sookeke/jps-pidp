export const lookupConfigKeys = [
  'colleges',
  'countries',
  'provinces',
  'organizations',
  'healthAuthorities',
  'justiceSectors',
  'crownRegions',
  'lawEnforcements',
  'correctionServices',
  'lawSocieties',
] as const;

export type LookupConfigKey = typeof lookupConfigKeys[number];

export type ILookupConfig = {
  [K in LookupConfigKey]: Lookup<string | number>[];
};

export interface LookupConfig extends ILookupConfig {
  accessTypes: Lookup[];
  colleges: CollegeLookup[];
  countries: Lookup<string>[];
  provinces: ProvinceLookup[];
  organizations: Lookup[];
  healthAuthorities: Lookup[];
  justiceSectors: Lookup[];
  crownRegions: CrownRegionLookup[];
  lawEnforcements: Lookup[];
  correctionServices: Lookup[];
  lawSocieties: Lookup[];
}

export interface Lookup<T extends number | string = number> {
  code: T;
  name: string;
}

export interface ProvinceLookup extends Lookup<string> {
  countryCode: string;
}

export interface CrownRegionLookup extends Lookup<string> {
  regionName: string;
  regionId: number;
  crownLocation: string;
  agencyCode: string;
}

export interface CollegeLookup extends Lookup<number> {
  acronym: string;
}
