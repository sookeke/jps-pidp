import { Injectable } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  ValidatorFn,
  Validators,
} from '@angular/forms';

import {
  AddressLine,
  AddressMap,
  Country,
  Province,
} from '@bcgov/shared/data-access';

import { LoggerService } from './logger.service';

@Injectable({
  providedIn: 'root',
})
export class FormUtilsServiceService {
  public constructor(private fb: FormBuilder, private logger: LoggerService) {}

  /**
   * @description
   * Checks the validity of a form, and triggers validation messages when invalid.
   */
  public checkValidity(form: FormGroup | FormArray): boolean {
    if (form.valid) {
      return true;
    } else {
      this.logFormErrors(form);

      form.markAllAsTouched();
      return false;
    }
  }

  /**
   * @description
   * Helper for quickly logging form errors.
   */
  public logFormErrors(form: FormGroup | FormArray): void {
    const formErrors = this.getFormErrors(form);
    if (formErrors) {
      this.logger.error('FORM_INVALID', formErrors);
    }
  }

  /**
   * @description
   * Get all the errors contained within a form.
   */
  public getFormErrors(
    form: FormGroup | FormArray
  ): { [key: string]: unknown } | null {
    if (!form) {
      return null;
    }

    let hasError = false;
    const result = Object.keys(form?.controls).reduce((acc, key) => {
      const control = form.get(key);

      const errors =
        control instanceof FormGroup || control instanceof FormArray
          ? this.getFormErrors(control)
          : control?.errors;

      if (errors) {
        acc[key] = errors;
        hasError = true;
      }

      return acc;
    }, {} as { [key: string]: unknown });
    return hasError ? result : null;
  }

  /**
   * @description
   * Provide an address form group.
   *
   * @param options available for manipulating the form group
   *  areRequired control names that are required
   *  areDisabled control names that are disabled
   *  useDefaults for province and country, otherwise empty
   *  exclude control names that are not needed
   */
  public buildAddressForm(
    options: {
      areRequired?: AddressLine[];
      areDisabled?: AddressLine[];
      useDefaults?: Extract<AddressLine, 'provinceCode' | 'countryCode'>[];
      exclude?: AddressLine[];
    } | null = null
  ): FormGroup {
    const controlsConfig: AddressMap<unknown[]> = {
      id: [0, []],
      street: [{ value: null, disabled: false }, []],
      city: [{ value: null, disabled: false }, []],
      provinceCode: [{ value: null, disabled: false }, []],
      countryCode: [{ value: null, disabled: false }, []],
      postal: [{ value: null, disabled: false }, []],
    };

    (Object.keys(controlsConfig) as AddressLine[])
      .filter((key: AddressLine) => !options?.exclude?.includes(key))
      .forEach((key: AddressLine) => {
        const control = controlsConfig[key];
        const controlProps = control[0] as {
          value: unknown;
          disabled: boolean;
        };
        const controlValidators = control[1] as Array<ValidatorFn>;

        if (options?.areDisabled?.includes(key)) {
          controlProps.disabled = true;
        }

        const useDefaults = options?.useDefaults;
        if (useDefaults) {
          if (key === 'provinceCode') {
            controlProps.value = Province.BRITISH_COLUMBIA;
          } else if (key === 'countryCode') {
            controlProps.value = Country.CANADA;
          }
        }

        if (options?.areRequired?.includes(key)) {
          controlValidators.push(Validators.required);
        }
      });

    return this.fb.group(controlsConfig);
  }
}
