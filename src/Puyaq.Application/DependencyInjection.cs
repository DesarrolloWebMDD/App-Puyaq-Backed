using Microsoft.Extensions.DependencyInjection;
using Puyaq.Application.Authentication.Interfaces;
using Puyaq.Application.Authentication.Services;

namespace Puyaq.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        return services;
    }
}
