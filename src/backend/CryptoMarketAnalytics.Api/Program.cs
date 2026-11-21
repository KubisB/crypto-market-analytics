using CryptoMarketAnalytics.Api.Binance.Client;
using CryptoMarketAnalytics.Api.Binance.Configuration;
using CryptoMarketAnalytics.Api.Configuration;
using CryptoMarketAnalytics.Api.Dtos;
using CryptoMarketAnalytics.Api.Persistence;
using CryptoMarketAnalytics.Api.Repositories;
using CryptoMarketAnalytics.Api.Services;
using CryptoMarketAnalytics.Api.Synchronization;
using CryptoMarketAnalytics.Api.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(api =>
    {
        // Missing or malformed query parameters get the same error shape, with a code, as the other errors.
        api.InvalidModelStateResponseFactory = context =>
        {
            var problem = ApiProblemDetails.FromValidation(new ValidationProblemDetails(context.ModelState));
            return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
        };
    });
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
builder.Services.AddOptions<StorageOptions>()
    .Bind(builder.Configuration.GetSection(StorageOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<SynchronizationOptions>()
    .Bind(builder.Configuration.GetSection(SynchronizationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpClient<IBinanceClient, BinanceClient>((services, client) =>
{
    var binance = services.GetRequiredService<IOptions<BinanceOptions>>().Value;
    client.Timeout = binance.Timeout;
    client.DefaultRequestHeaders.UserAgent.ParseAdd(binance.UserAgent);
});

builder.Services.AddDbContext<MarketDbContext>((services, db) =>
{
    var storage = services.GetRequiredService<IOptions<StorageOptions>>().Value;
    var environment = services.GetRequiredService<IWebHostEnvironment>();
    db.UseSqlite($"Data Source={storage.GetFullDatabasePath(environment.ContentRootPath)}");
});
builder.Services.AddScoped<IDailyCandleRepository, SqliteDailyCandleRepository>();
builder.Services.AddScoped<ISyncStateRepository, SqliteSyncStateRepository>();

builder.Services.AddSingleton<DailyCandleValidator>();
builder.Services.AddScoped<DailyCandleSynchronizer>();
builder.Services.AddHostedService<DailyCandleSyncBackgroundService>();

builder.Services.AddScoped<IDailyCandleService, DailyCandleService>();
builder.Services.AddScoped<IDataStatusService, DataStatusService>();

var app = builder.Build();

app.MigrateDatabase();

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
