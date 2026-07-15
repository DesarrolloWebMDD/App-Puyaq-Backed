using System.Globalization;
using System.Resources;

namespace Puyaq.Repository.Resources;

internal static class StoreProcedures
{
    private static readonly ResourceManager ResourceManager =
        new("Puyaq.Repository.Resources.StoreProcedures", typeof(StoreProcedures).Assembly);

    private static string Get(string name) =>
        ResourceManager.GetString(name, CultureInfo.InvariantCulture)
        ?? throw new InvalidOperationException($"No existe el recurso StoreProcedures:{name}.");

    internal static string AuthenticationGetByNormalizedEmail => Get(nameof(AuthenticationGetByNormalizedEmail));
    internal static string AuthenticationRegister => Get(nameof(AuthenticationRegister));
    internal static string AuthenticationUpdateLastLogin => Get(nameof(AuthenticationUpdateLastLogin));
    internal static string AuthenticationSaveRefreshToken => Get(nameof(AuthenticationSaveRefreshToken));
    internal static string AuthenticationGetByExternalLogin => Get(nameof(AuthenticationGetByExternalLogin));
    internal static string AuthenticationRegisterExternalUser => Get(nameof(AuthenticationRegisterExternalUser));
    internal static string AuthenticationUpdateExternalLogin => Get(nameof(AuthenticationUpdateExternalLogin));
    internal static string AuthenticationUpdateExternalLoginLastLogin => Get(nameof(AuthenticationUpdateExternalLoginLastLogin));
    internal static string AuthenticationGetUserById => Get(nameof(AuthenticationGetUserById));
    internal static string AuthenticationGetExternalProviders => Get(nameof(AuthenticationGetExternalProviders));
    internal static string AuthenticationLinkExternalProvider => Get(nameof(AuthenticationLinkExternalProvider));
    internal static string AuthenticationCountMethods => Get(nameof(AuthenticationCountMethods));
    internal static string AuthenticationUnlinkExternalProvider => Get(nameof(AuthenticationUnlinkExternalProvider));

}
