using Config.Server.Api.Http.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Config.Server.Api.Http.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services)
    {
        IOptions<JwtAuthOptions> options = services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<JwtAuthOptions>>();

        JwtAuthOptions jwtConfig = options.Value;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.Authority = jwtConfig.Authority;
                jwtOptions.Audience = jwtConfig.Audience;
                jwtOptions.RequireHttpsMetadata = false;

                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = jwtConfig.Audience,
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };

                jwtOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey(jwtConfig.AccessTokenCookieName))
                            context.Token = context.Request.Cookies[jwtConfig.AccessTokenCookieName];

                        return Task.CompletedTask;
                    },
                };
            });

        return services;
    }
}