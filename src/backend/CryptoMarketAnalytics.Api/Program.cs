using CryptoMarketAnalytics.Api.Binance.Client;
using CryptoMarketAnalytics.Api.Binance.Configuration;
using CryptoMarketAnalytics.Api.Configuration;
using CryptoMarketAnalytics.Api.Repositories;
using CryptoMarketAnalytics.Api.Services;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

// Settings are validated at startup, so a missing or invalid value stops the application immediately.
builder.Services.AddOptions<MarketOptions>()
    .Bind(builder.Configuration.GetSection(MarketOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(market => market.AnalysisStartDate != default, "Market:AnalysisStartDate is required.")
    .Validate(market => market.Symbols.All(symbol => !string.IsNullOrWhiteSpace(symbol)), "Market:Symbols must not contain empty values.")
    .ValidateOnStart();
builder.Services.AddOptions<BinanceOptions>()
    .Bind(builder.Configuration.GetSection(BinanceOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpClient<IBinanceClient, BinanceClient>((services, client) =>
{
    var binance = services.GetRequiredService<IOptions<BinanceOptions>>().Value;
    client.Timeout = binance.Timeout;
    client.DefaultRequestHeaders.UserAgent.ParseAdd(binance.UserAgent);
});
builder.Services.AddSingleton<IDailyCandleRepository, InMemoryDailyCandleRepository>();
builder.Services.AddScoped<IDailyCandleService, DailyCandleService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();
