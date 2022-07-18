import { IdentityProvider } from '../enums/identity-provider.enum';
import { UserIdentity } from './user-identity.model';
import { IUserResolver, User } from './user.model';

export class BcpsUser implements User {
  public readonly identityProvider: IdentityProvider;
  //public hpdid: string;
  public userId: string;
  public firstName: string;
  public lastName: string;
  public idir: string;

  public constructor({ accessTokenParsed, brokerProfile }: UserIdentity) {
    const {
      firstName,
      lastName,
      //username: hpdid,
    } = brokerProfile;
    const {
      identity_provider,
      sub: userId,
      preferred_username: idir,
    } = accessTokenParsed;

    this.identityProvider = identity_provider;
    //this.hpdid = hpdid;
    this.userId = userId;
    this.firstName = firstName;
    this.lastName = lastName;
    this.idir = idir;
  }
}

export class BcpsResolver implements IUserResolver<BcpsUser> {
  public constructor(public userIdentity: UserIdentity) {}
  public resolve(): BcpsUser {
    return new BcpsUser(this.userIdentity);
  }
}
