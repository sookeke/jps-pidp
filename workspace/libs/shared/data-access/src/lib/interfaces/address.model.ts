export type AddressLine = Exclude<keyof Address, 'id'>;
export type AddressType =
  | 'verifiedAddress'
  | 'physicalAddress'
  | 'mailingAddress';

export const addressTypes: AddressType[] = [
  'verifiedAddress',
  'mailingAddress',
  'physicalAddress',
];

/**
 * @description
 * List of optional address line items.
 */
export const optionalAddressLineItems: (keyof Address)[] = ['id'];

export class Address {
  public constructor(
    public countryCode: string | null = null,
    public provinceCode: string | null = null,
    public street: string | null = null,
    public city: string | null = null,
    public postal: string | null = null,
    public id: number = 0
  ) {
    this.street = street;
    this.city = city;
    this.provinceCode = provinceCode;
    this.countryCode = countryCode;
    this.postal = postal;
  }

  /**
   * @description
   * Create an new instance of an Address.
   *
   * NOTE: Useful for converting object literals (or data
   * transfer objects) into an instance.
   */
  public static instanceOf(address: Address): Address {
    const {
      id = 0,
      street = null,
      city = null,
      provinceCode = null,
      countryCode = null,
      postal = null,
    } = address;
    return new Address(countryCode, provinceCode, street, city, postal, id);
  }
}
