import { IdentityProvider } from '../enums/identity-provider.enum';
import { UserIdentity } from './user-identity.model';
import { IUserResolver, User } from './user.model';

export class BcpsUser implements User {
  public readonly identityProvider: IdentityProvider;
  public hpdid: string;
  public userId: string;
  public firstName: string;
  public lastName: string;
  public birthdate: string;

  public constructor({ accessTokenParsed, brokerProfile }: UserIdentity) {
    const {
      firstName,
      lastName,
      username: hpdid,
      attributes: {
        birthdate: [birthdate],
      },
    } = brokerProfile;
    const { identity_provider, sub: userId } = accessTokenParsed;

    this.identityProvider = identity_provider;
    this.hpdid = hpdid;
    this.userId = userId;
    this.firstName = firstName;
    this.lastName = lastName;
    this.birthdate = birthdate;
  }
}

export class BcpsResolver implements IUserResolver<BcpsUser> {
  public constructor(public userIdentity: UserIdentity) {}
  public resolve(): BcpsUser {
    return new BcpsUser(this.userIdentity);
  }
}
