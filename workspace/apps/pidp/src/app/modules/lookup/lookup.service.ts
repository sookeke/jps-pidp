import { Injectable } from '@angular/core';

import { Observable, map, of } from 'rxjs';

import { SortUtils } from '@bcgov/shared/utils';

import { LookupResource } from './lookup-resource.service';
import {
  CollegeLookup,
  Lookup,
  LookupConfig,
  CrownRegionLookup,
  ProvinceLookup,
} from './lookup.types';

export interface ILookupService extends LookupConfig {
  load(): Observable<LookupConfig | null>;
}

@Injectable({
  providedIn: 'root',
})
export class LookupService implements ILookupService {
  private lookupConfig: LookupConfig | null;

  public constructor(private lookupResource: LookupResource) {
    this.lookupConfig = null;
  }
  public get accessTypes(): Lookup[] {
    return this.copyAndSortByKey(this.lookupConfig?.accessTypes);
  }

  public get colleges(): CollegeLookup[] {
    return this.copyAndSortByKey<CollegeLookup>(this.lookupConfig?.colleges);
  }

  public get countries(): Lookup<string>[] {
    return this.copyAndSortByKey<Lookup<string>>(
      this.lookupConfig?.countries,
      'name'
    );
  }



  public get provinces(): ProvinceLookup[] {
    return this.copyAndSortByKey<ProvinceLookup>(
      this.lookupConfig?.provinces,
      'name'
    );
  }

  public get organizations(): Lookup[] {
    return this.copyAndSortByKey(this.lookupConfig?.organizations);
  }

  public get healthAuthorities(): Lookup[] {
    return this.copyAndSortByKey(this.lookupConfig?.healthAuthorities);
  }
  public get justiceSectors(): Lookup[] {
    return this.copyAndSortByKey(this.lookupConfig?.justiceSectors);
  }

  public get crownRegions(): CrownRegionLookup[] {
    return this.copyAndSortByKey<CrownRegionLookup>(
      this.lookupConfig?.crownRegions,
      'crownLocation'
    );  }

  public get lawEnforcements(): Lookup[] {
    return this.copyAndSortByKey(this.lookupConfig?.lawEnforcements);
  }

  public get correctionServices(): Lookup[] {
    return this.copyAndSortByKey(this.lookupConfig?.correctionServices);
  }
  public get lawSocieties(): Lookup[] {
    return this.copyAndSortByKey(this.lookupConfig?.lawSocieties);
  }

  /**
   * @description
   * Load the runtime lookups, otherwise use a locally
   * cached version of the lookups.
   */
  public load(): Observable<LookupConfig | null> {
    return !this.lookupConfig
      ? this.lookupResource
          .getLookups()
          .pipe(
            map(
              (lookupConfig: LookupConfig | null) =>
                (this.lookupConfig = lookupConfig)
            )
          )
      : of({ ...this.lookupConfig });
  }

  /**
   * @description
   * Make a copy of the lookup so it won't be overwritten by
   * reference within the service, and then sort by key.
   */
  private copyAndSortByKey<T = Lookup>(
    lookup: T[] | undefined,
    sortBy: keyof T = 'code' as keyof T
  ): T[] {
    return lookup?.length
      ? [...lookup].sort(SortUtils.sortByKey<T>(sortBy))
      : [];
  }
}
