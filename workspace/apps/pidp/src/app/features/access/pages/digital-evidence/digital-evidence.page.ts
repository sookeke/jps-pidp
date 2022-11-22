import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Component, Inject, Input, OnInit } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';

import {
  EMPTY,
  Observable,
  catchError,
  first,
  map,
  noop,
  of,
  take,
  tap,
} from 'rxjs';

import { APP_CONFIG, AppConfig } from '@app/app.config';
import { AbstractFormPage } from '@app/core/classes/abstract-form-page.class';
import { PartyService } from '@app/core/party/party.service';
import { DocumentService } from '@app/core/services/document.service';
import { LoggerService } from '@app/core/services/logger.service';
import { IdentityProvider } from '@app/features/auth/enums/identity-provider.enum';
import { AccessTokenService } from '@app/features/auth/services/access-token.service';
import { AuthorizedUserService } from '@app/features/auth/services/authorized-user.service';
import { StatusCode } from '@app/features/portal/enums/status-code.enum';

import { FormUtilsService } from '@core/services/form-utils.service';

import { PartyUserTypeResource } from '../../../../features/admin/shared/usertype-resource.service';
import { OrganizationUserType } from '../../../../features/admin/shared/usertype-service.model';
import { BcpsAuthResourceService } from './auth/bcps-auth-resource.service';
import { DigitalEvidenceFormState } from './digital-evidence-form-state';
import { DigitalEvidenceResource } from './digital-evidence-resource.service';
import {
  digitalEvidenceSupportEmail,
  digitalEvidenceUrl,
} from './digital-evidence.constants';
import { AssignedRegion } from './digital-evidence-account.model';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-digital-evidence',
  templateUrl: './digital-evidence.page.html',
  styleUrls: ['./digital-evidence.page.scss'],
})
export class DigitalEvidencePage
  extends AbstractFormPage<DigitalEvidenceFormState>
  implements OnInit
{
  public formState: DigitalEvidenceFormState;
  public title: string;

  public organizationType: OrganizationUserType;
  public assignedRegions: AssignedRegion[] = [];
  public digitalEvidenceUrl: string;
  public dataSource: MatTableDataSource<AssignedRegion>;

  public identityProvider$: Observable<IdentityProvider>;
  //public userType$: Observable<OrganizationUserType[]>;
  public IdentityProvider = IdentityProvider;

  public collectionNotice: string;
  public completed: boolean | null;
  public pending: boolean | null;
  public policeAgency: Observable<string>;
  public result: string;
  public accessRequestFailed: boolean;
  public digitalEvidenceSupportEmail: string;
  //@Input() public form!: FormGroup;
  public formControlNames: string[];
  public selectedOption = 0;
  public displayedColumns: string[] = [
    'regionName',
    'assignedAgency'
  ];
  public userTypes = [
    { id: 0, name: '--Select User Type--', disable: true },
    { id: 1, name: 'CorrectionService', disable: false },
    { id: 2, name: 'JusticeSector', disable: false },
    { id: 3, name: 'LawEnforcement', disable: false },
    { id: 4, name: 'LawSociety', disable: false },
  ];
  public constructor(
    @Inject(APP_CONFIG) private config: AppConfig,
    private route: ActivatedRoute,
    protected dialog: MatDialog,
    private router: Router,

    protected formUtilsService: FormUtilsService,
    private partyService: PartyService,
    private resource: DigitalEvidenceResource,
    private usertype: PartyUserTypeResource,
    private userOrgunit: BcpsAuthResourceService,
    private logger: LoggerService,
    documentService: DocumentService,
    accessTokenService: AccessTokenService,
    private authorizedUserService: AuthorizedUserService,
    fb: FormBuilder
  ) {
    super(dialog, formUtilsService);
    const routeData = this.route.snapshot.data;
    this.title = routeData.title;
    this.digitalEvidenceUrl = digitalEvidenceUrl;
    this.organizationType = new OrganizationUserType();
    const partyId = this.partyService.partyId;
    this.dataSource = new MatTableDataSource();

    //this.userType$ = this.usertype.getUserType(partyId);
    this.identityProvider$ = this.authorizedUserService.identityProvider$;
    this.result = '';
    this.policeAgency = accessTokenService
      .decodeToken()
      .pipe(map((token) => token?.identity_provider ?? ''));

    accessTokenService.decodeToken().subscribe((n) => {
      console.log(n.identity_provider);
      this.result = n.identity_provider;
    });
    this.usertype.getUserType(partyId).subscribe((data: any) => {
      this.organizationType.organizationType = data['organizationType'];
      this.organizationType.participantId = data['participantId'];
      this.organizationType.organizationName = data['organizationName'];

      this.formState.OrganizationName.patchValue(
        this.organizationType.organizationName
      );



      this.formState.OrganizationType.patchValue(
        this.organizationType.organizationType
      );
      this.formState.ParticipantId.patchValue(
        this.organizationType.participantId
      );

      this.userOrgunit.getUserOrgUnit(partyId,Number(this.organizationType.participantId )).subscribe((data:any) => {
        this.assignedRegions = data;
        this.formState.AssignedRegions.patchValue(this.assignedRegions);
      });

    });



    this.formState = new DigitalEvidenceFormState(fb);
    this.collectionNotice =
      documentService.getDigitalEvidenceCollectionNotice();
    this.completed =
      routeData.digitalEvidenceStatusCode === StatusCode.COMPLETED;
    this.pending = routeData.digitalEvidenceStatusCode === StatusCode.PENDING;
    this.accessRequestFailed = false;
    this.digitalEvidenceSupportEmail = digitalEvidenceSupportEmail;
    this.formControlNames = [
      'OrganizationType',
      'OrganizationName',
      'AssignedRegions',
      'ParticipantId',
    ];
  }

  public onBack(): void {
    this.navigateToRoot();
  }
  protected performSubmission(): Observable<void> {
    const partyId = this.partyService.partyId;
    console.log('Submiting');
    console.log(this.formState.ParticipantId.value);
    if (this.selectedOption == 1) {
      return partyId && this.formState.json
        ? this.resource.requestAccess(
            partyId,
            this.formState.OrganizationType.value,
            this.formState.OrganizationName.value,
            this.formState.ParticipantId.value,
            this.formState.AssignedRegions.value
          )
        : EMPTY;
    }

    return partyId && this.formState.json
      ? this.resource.requestAccess(
          partyId,
          this.formState.OrganizationType.value,
          this.formState.OrganizationName.value,
          this.formState.ParticipantId.value,
          this.formState.AssignedRegions.value

        )
      : EMPTY;
  }
  public showFormControl(formControlName: string): boolean {
    return this.formControlNames.includes(formControlName);
  }
  // public get userType(): FormControl {
  //   return this.form.get('userType') as FormControl;
  // }
  public onChange(data: number): void {
    //const element = event.currentTarget as HTMLInputElement;
    //const value = element.value;

    this.selectedOption = data;
    // this.formState.pidNumber.clearValidators();
    // this.formState.pidNumber.reset();
    // this.formState.agency.clearValidators();
    // this.formState.agency.reset();
    // //this.formState.userType.clearValidators();
    // this.formState.ikeyCertCode.clearValidators();
    // this.formState.ikeyCertCode.reset();
    // if (this.selectedOption == 1) {
    //   this.formState.ikeyCertCode.setValidators([Validators.required]);
    //   console.log(this.formState.ikeyCertCode.value);
    //   //this.formState.pidNumber = this.formState.ikeyCertCode;
    //   console.log(this.formState.pidNumber.value);
    // }
    // if (this.selectedOption == 2) {
    //   this.formState.pidNumber.setValidators([Validators.required]);
    // }
    // if (this.selectedOption == 3) {
    //   //let result: string;
    //   //this.policeAgency.subscribe((val) => (result = val));
    //   //this.policeAgency.pipe(first()).subscribe((n) => (this.result = n));
    //   const index = this.agencies.findIndex((p) => p.name == this.result);
    //   this.formState.agency.patchValue(index);
    //   this.formState.agency.setValidators([Validators.required]);
    // }
  }
  // public OnSubmit(): void {
  //   console.log('Form Submitted');
  //   console.log(this.form.value);
  // }
  public onRequestAccess(): void {
    console.log('Submiting');
    console.log(this.formState.ParticipantId.value);
    if (this.selectedOption == 1) {
      this.resource
        .requestAccess(
          this.partyService.partyId,
          this.formState.OrganizationType.value,
          this.formState.OrganizationName.value,
          this.formState.ParticipantId.value,
          this.formState.AssignedRegions.value
        )
        .pipe(
          tap(() => (this.completed = true)),
          catchError((error: HttpErrorResponse) => {
            if (error.status === HttpStatusCode.NotFound) {
              this.navigateToRoot();
            }
            this.accessRequestFailed = true;
            return of(noop());
          })
        )
        .subscribe();
    } else {
      this.resource
        .requestAccess(
          this.partyService.partyId,
          this.formState.OrganizationType.value,
          this.formState.OrganizationName.value,
          this.formState.ParticipantId.value,
          this.formState.AssignedRegions.value

        )
        .pipe(
          tap(() => (this.completed = true)),
          catchError((error: HttpErrorResponse) => {
            if (error.status === HttpStatusCode.NotFound) {
              this.navigateToRoot();
            }
            this.accessRequestFailed = true;
            return of(noop());
          })
        )
        .subscribe();
    }
  }

  public ngOnInit(): void {
    const partyId = this.partyService.partyId;
    this.formState.OrganizationName.patchValue(
      this.organizationType.organizationName
    );
    this.formState.OrganizationType.patchValue(
      this.organizationType.organizationType
    );
    this.formState.ParticipantId.patchValue(
      this.organizationType.participantId
    );
    // this.form = new FormGroup({
    //   userType: new FormControl('', [Validators.required]),
    //   ikeyCertCode: new FormControl('', [Validators.required]),
    //   pidNumber: new FormControl('', [Validators.required]),
    // });

    if (!partyId) {
      this.logger.error('No party ID was provided');
      return this.navigateToRoot();
    }

    if (this.completed === null) {
      this.logger.error('No status code was provided');
      return this.navigateToRoot();
    }
  }

  private navigateToRoot(): void {
    this.router.navigate([this.route.snapshot.data.routes.root]);
  }
}
