using Microsoft.Extensions.DependencyInjection;
using Puyaq.CrossCutting.Time;
using Puyaq.Repository.Implementation;
using Puyaq.Repository.Interface;

namespace Puyaq.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.AddScoped<IExternalIdentityProvider, ExternalIdentityProvider>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
      
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
