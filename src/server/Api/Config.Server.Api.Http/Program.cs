#pragma warning disable CA1506

using Config.Server.Api.Http.Extensions;
using Config.Server.Application.Extensions;
using Config.Server.Infrastructure.Persistence.Extensions;
using Config.Server.Infrastructure.Persistence.Options;
using Config.Server.Infrastructure.Security.Extensions;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder();

builder.Services.Configure<ConnectionOptions>(builder.Configuration);

builder.Services.AddMigrations();
builder.Services.AddInfrastructure();
builder.Services.AddRsaKey();
builder.Services.AddJwtGenerator();
builder.Services.AddApplication();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddJwtAuth();
builder.Services.AddProjectRoleAuthorization();

WebApplication app = builder.Build();

app.MapGet(
    "/.well-known/jwks.json",
    (RsaSecurityKey key) =>
    {
        JsonWebKey jwk = JsonWebKeyConverter.ConvertFromSecurityKey(key);

        return Results.Json(new
        {
            keys = new[]
            {
                new
                {
                    kty = jwk.Kty,
                    use = "sig",
                    kid = jwk.Kid,
                    alg = "RS256",
                    n = jwk.N,
                    e = jwk.E,
                },
            },
        });
    });

app.MapGet(
    "/.well-known/openid-configuration",
    () => Results.Json(new
    {
        issuer = "config-server",
        jwks_uri = "http://localhost:5150/.well-known/jwks.json",
        response_types_supported = new List<string> { "id_token" },
        subject_types_supported = new List<string> { "public" },
        id_token_signing_alg_values_supported = new List<string> { "RS256" },
    }));

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

await app.Services.MigrationsUp();

await app.RunAsync();