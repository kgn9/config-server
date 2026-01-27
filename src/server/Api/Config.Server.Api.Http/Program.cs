using Config.Server.Application.Extensions;
using Config.Server.Infrastructure.Persistence.Extensions;
using Config.Server.Infrastructure.Persistence.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder();

builder.Services.Configure<ConnectionOptions>(builder.Configuration);

builder.Services.AddMigrations();
builder.Services.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

await app.Services.MigrationsUp();

await app.RunAsync();