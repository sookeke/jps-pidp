import { FormBuilder, FormControl, Validators } from '@angular/forms';

import { AbstractFormState, FormControlValidators } from '@bcgov/shared/ui';

import { DemsAccount } from './digital-evidence-account.model';

export class DigitalEvidenceFormState extends AbstractFormState<DemsAccount> {
  public constructor(private fb: FormBuilder) {
    super();

    this.buildForm();
  }

  public get userType(): FormControl {
    return this.formInstance.get('userType') as FormControl;
  }
  public get agency(): FormControl {
    return this.formInstance.get('agency') as FormControl;
  }
  public get pidNumber(): FormControl {
    return this.formInstance.get('pidNumber') as FormControl;
  }
  public get ikeyCertCode(): FormControl {
    return this.formInstance.get('ikeyCertCode') as FormControl;
  }
  public get json(): DemsAccount | undefined {
    if (!this.formInstance) {
      return;
    }

    return this.formInstance.getRawValue();
  }

  public patchValue(model: DemsAccount | null): void {
    if (!this.formInstance || !model) {
      return;
    }

    this.formInstance.patchValue(model);
  }

  public buildForm(): void {
    this.formInstance = this.fb.group({
      userType: ['', [Validators.required]],
      ikeyCertCode: [''],
      pidNumber: [''],
      agency: ['', [Validators.required]],
    });
  }
}
