using Config.Server.Application.Abstractions.Identity;
using Config.Server.Infrastructure.Identity.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Config.Server.Infrastructure.Identity.Extensions;

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
}