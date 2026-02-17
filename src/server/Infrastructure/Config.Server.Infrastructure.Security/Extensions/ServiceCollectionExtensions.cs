using Config.Server.Application.Abstractions.Identity;
using Config.Server.Application.Models.Enums;
using Config.Server.Infrastructure.Security.Generators;
using Config.Server.Infrastructure.Security.Handlers;
using Config.Server.Infrastructure.Security.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        services.AddScoped<IApiKeyGenerator, ApiKeyGenerator>();

        return services;
    }

    public static IServiceCollection AddProjectRoleAuthorization(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthorizationHandler, ProjectRoleHandler>();

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme, "ClientKey")
                .RequireAuthenticatedUser().Build();

            options.AddPolicy("CanDelete", policy =>
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "ClientKey")
                    .Requirements.Add(new ProjectRoleRequirement(ProjectRoles.Maintainer)));

            options.AddPolicy("CanEdit", policy =>
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "ClientKey")
                    .Requirements.Add(new ProjectRoleRequirement(ProjectRoles.Editor)));

            options.AddPolicy("CanRead", policy =>
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "ClientKey")
                    .Requirements.Add(new ProjectRoleRequirement(ProjectRoles.Reader)));

            options.AddPolicy("CanAssignRoles", policy =>
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "ClientKey")
                    .Requirements.Add(new ProjectRoleRequirement(ProjectRoles.Maintainer)));
        });

        return services;
    }

    public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationHandler, ApiKeyHandler>();

        // TODO Unhardcode
        services.AddAuthentication().AddScheme<ApiKeyAuthenticationOptions, ApiKeyHandler>("ClientKey", null);

        return services;
    }
}