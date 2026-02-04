using Config.Server.Application.Abstractions.Identity;
using Config.Server.Application.Models.Enums;
using Config.Server.Infrastructure.Security.Handlers;
using Config.Server.Infrastructure.Security.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Config.Server.Infrastructure.Security.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRsaKey(this IServiceCollection services)
    {
        services.AddSingleton<RsaSecurityKey>(provider =>
        {
            IOptions<JwtOptions> jwtOptions = provider.GetRequiredService<IOptions<JwtOptions>>();
            var rsa = RSA.Create(jwtOptions.Value.KeySize);

            return new RsaSecurityKey(rsa) { KeyId = jwtOptions.Value.KeyId };
        });

        return services;
    }

    public static IServiceCollection AddJwtGenerator(this IServiceCollection services)
    {
        services.AddScoped<IJwtGenerator, JwtGenerator>();

        return services;
    }

    public static IServiceCollection AddProjectRoleAuthorization(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthorizationHandler, ProjectRoleHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("CanDelete", policy =>
                policy.Requirements.Add(new ProjectRoleRequirement(ProjectRoles.Maintainer)));

            options.AddPolicy("CanEdit", policy =>
                policy.Requirements.Add(new ProjectRoleRequirement(ProjectRoles.Editor)));

            options.AddPolicy("CanRead", policy =>
                policy.Requirements.Add(new ProjectRoleRequirement(ProjectRoles.Reader)));
        });

        return services;
    }
}