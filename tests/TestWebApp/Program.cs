using Config.Server.Provider.Extensions;
using Microsoft.Extensions.Options;
using TestWebApp;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TestConfig>(builder.Configuration);
builder.Services.AddConfigServer(builder.Configuration);

WebApplication app = builder.Build();

app.UseMiddlewareRefreshing();

app.MapGet(
    "/hello",
    (HttpContext context) =>
    {
        IOptionsSnapshot<TestConfig> options = context.RequestServices.GetRequiredService<IOptionsSnapshot<TestConfig>>();
        return options.Value.Line;
    });

app.Run();