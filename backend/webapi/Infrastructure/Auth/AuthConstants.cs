namespace Pidp.Infrastructure.Auth;

public static class Claims
{
    public const string Address = "address";
    public const string AssuranceLevel = "identity_assurance_level";
    public const string Birthdate = "birthdate";
    public const string Gender = "gender";
    public const string Email = "email";
    public const string FamilyName = "family_name";
    public const string GivenName = "given_name";
    public const string GivenNames = "given_names";
    public const string IdentityProvider = "identity_provider";
    public const string PreferredUsername = "preferred_username";
    public const string ResourceAccess = "resource_access";
    public const string Subject = "sub";
    public const string Roles = "roles";
}

public static class DefaultRoles
{
    public const string Bcps = "BCPS";
}

public static class ClaimValues
{
    public const string BCServicesCard = "bcsc";
    public const string Idir = "idir";
    public const string Phsa = "phsa";
    public const string Bcps = "adfscert";
    public const string Adfs = "adfs"; // test

}

public static class Policies
{
    public const string BcscAuthentication = "bcsc-authentication-policy";
    public const string IdirAuthentication = "idir-authentication-policy";
    public const string AnyPartyIdentityProvider = "party-idp-policy";
    public const string UserOwnsResource = "user-owns-resource-policy";
    public const string AllDemsIdentityProvider = "dems-idp-policy";
    public const string BcpsAuthentication = "bcps-authentication-policy";
    public const string AdminAuthentication = "admin-authentication-policy";
}

public static class Clients
{
    public const string PidpApi = "PIDP-SERVICE";
}

public static class Roles
{
    // PIdP Role Placeholders
    public const string Admin = "ADMIN";
    public const string User = "USER";
}
