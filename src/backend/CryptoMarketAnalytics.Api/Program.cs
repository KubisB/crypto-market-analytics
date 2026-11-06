using CryptoMarketAnalytics.Api.Binance;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.Configure<BinanceOptions>(builder.Configuration.GetSection(BinanceOptions.SectionName));
builder.Services.AddHttpClient<IBinanceClient, BinanceClient>((services, client) =>
{
    client.Timeout = services.GetRequiredService<IOptions<BinanceOptions>>().Value.Timeout;
    client.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoMarketAnalytics/1.0");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();
