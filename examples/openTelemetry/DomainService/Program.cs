using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TelemetryExampleServices;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Plugins;
using ZiggyCreatures.Caching.Fusion.Plugins.Metrics.Core;
using ZiggyCreatures.Caching.Fusion.Plugins.Metrics.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);
var serviceName = "OtelDomainService";

// OpenTelemetry
var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(
        serviceName,
        serviceVersion: assemblyVersion,
        serviceInstanceId: Environment.MachineName);

// Traces
builder.Services.AddOpenTelemetryTracing(options =>
{
    options
        .SetResourceBuilder(resourceBuilder)
        .SetSampler(new AlwaysOnSampler())
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation();

    options.AddOtlpExporter(otlpOptions =>
    {
        otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint"));
    });

#if DEBUG
    options.AddConsoleExporter();
#endif
});

// For options which can be bound from IConfiguration.
builder.Services
    .Configure<AspNetCoreInstrumentationOptions>(
        builder.Configuration.GetSection("AspNetCoreInstrumentation")
    );

// Logging
builder.Logging.ClearProviders();

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(resourceBuilder);

    options.AddOtlpExporter(otlpOptions =>
    {
        otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint"));
    });

#if DEBUG
    options.AddConsoleExporter();
#endif
});

builder.Services.Configure<OpenTelemetryLoggerOptions>(opt =>
{
    opt.IncludeScopes = true;
    opt.ParseStateValues = true;
    opt.IncludeFormattedMessage = true;
});

// Metrics
var domainMeterName = "domain"; // same as cacheName

builder.Services.AddOpenTelemetryMetrics(options =>
{
    options.SetResourceBuilder(resourceBuilder)
        .AddMeter(domainMeterName)
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation();

    options.AddOtlpExporter(otlpOptions =>
    {
        otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint"));
    });
#if DEBUG
    options.AddConsoleExporter();
#endif
});


// Add services to the container.
var memoryCache = new MemoryCache(new MemoryCacheOptions());
builder.Services.AddSingleton(builder.Configuration.GetSection("CacheMetrics").Get<MetricsConfig>());

builder.Services.AddSingleton<IMemoryCache>(memoryCache);
builder.Services.AddSingleton<IFusionCachePlugin>(
    new FusionMeter(
        domainMeterName, 
        memoryCache,
        $"appMetrics_{serviceName}_cache_events"
        )
    );

builder.Services.AddFusionCache(options =>
    options.DefaultEntryOptions = new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromSeconds(60)
    }
        .SetFailSafe(true, TimeSpan.FromHours(1), TimeSpan.FromSeconds(5))
        .SetFactoryTimeouts(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
);




builder.Services.AddSingleton<IDataManager>(new DataManager());
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();