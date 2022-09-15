import { IdentityProvider } from '../enums/identity-provider.enum';
import { UserIdentity } from './user-identity.model';
import { IUserResolver, User } from './user.model';

export class BcscUser implements User {
  public readonly identityProvider: IdentityProvider;
  public jpdid: string;
  public userId: string;
  public firstName: string;
  public lastName: string;
  public birthdate: string;
  public gender: string;
  public email: string;

  public constructor({ accessTokenParsed, brokerProfile }: UserIdentity) {
    const {
      firstName,
      lastName,
      email,
      username: jpdid,
      attributes: {
        birthdate: [birthdate],
        gender: [gender],
      },
    } = brokerProfile;
    const { identity_provider, sub: userId } = accessTokenParsed;

    this.identityProvider = identity_provider;
    this.jpdid = jpdid;
    this.userId = userId;
    this.firstName = firstName;
    this.lastName = lastName;
    this.birthdate = birthdate;
    this.gender = gender;
    this.email = email;
  }
}

export class BcscResolver implements IUserResolver<BcscUser> {
  public constructor(public userIdentity: UserIdentity) {}
  public resolve(): BcscUser {
    return new BcscUser(this.userIdentity);
  }
}
