using Config.Server.Application.Extensions;
using Config.Server.Application.Models.Options;
using Config.Server.Infrastructure.Persistence.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder();

builder.Services.Configure<ConnectionOptions>(builder.Configuration.GetSection("Infrastructure"));

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

app.Run();