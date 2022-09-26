import { Router } from '@angular/router';

import { Observable } from 'rxjs';

import { AlertType } from '@bcgov/shared/ui';

import { ShellRoutes } from '@app/features/shell/shell.routes';
import { TrainingRoutes } from '@app/features/training/training.routes';

import { AdminRoutes } from '../../../admin/admin.routes';
import { StatusCode } from '../../enums/status-code.enum';
import { ProfileStatus } from '../../models/profile-status.model';
import { PortalSectionAction } from '../portal-section-action.model';
import { PortalSectionKey } from '../portal-section-key.type';
import { IPortalSection } from '../portal-section.model';

export class AdministratorPortalSection implements IPortalSection {
  public readonly key: PortalSectionKey;
  public heading: string;
  public description: string;

  public constructor(
    private profileStatus: ProfileStatus,
    private router: Router
  ) {
    this.key = 'administrationPanel';
    this.heading = 'Administration Panel';
    this.description = 'Manage Users, Review, Approve or Deny Access Requests.';
  }

  public get hint(): string {
    return '15 min to complete';
  }

  /**
   * @description
   * Get the properties that define the action on the section.
   */
  public get action(): PortalSectionAction {
    const demographicsStatusCode =
      this.profileStatus.status.demographics.statusCode;
    return {
      label: 'View',
      route: AdminRoutes.routePath(AdminRoutes.PARTIES),
      disabled: demographicsStatusCode !== StatusCode.COMPLETED,
    };
  }

  public get statusType(): AlertType {
    return 'info';
  }

  public get status(): string {
    //const statusCode = this.getStatusCode();
    return 'Available';
  }

  public performAction(): Observable<void> | void {
    this.router.navigate([ShellRoutes.routePath(this.action.route)]);
  }

  private getStatusCode(): StatusCode {
    // TODO remove null check once API exists
    return this.profileStatus.status.administratorInfo?.statusCode;
  }
}
