﻿using System;
using System.Configuration;
using System.Text.Json;
using System.Threading;
using App.Metrics;
using App.Metrics.Extensions.Hosting;
using App.Metrics.Filtering;
using App.Metrics.Formatters.InfluxDB;
using AppMetricsPluginExample.Services;
using JoeShook.FusionCache.AppMetrics.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using ZiggyCreatures.Caching.Fusion;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;


namespace AppMetricsPluginExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var emailCache = new MemoryCache(new MemoryCacheOptions());
            var hostNameCache = new MemoryCache(new MemoryCacheOptions());
            
            services.AddMvc()
                .AddJsonOptions(options =>

                    options.SerializerSettings.ContractResolver
                                       = new DefaultContractResolver()
                )
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            
            var appMetricsContextLabel = $"appMetrics_AppMetricsPluginExampleFrameworkOnAspNetCore";
            
            var appMetrics = new MetricsBuilder()
                .Configuration.Configure(
                    options =>
                    {
                        options.DefaultContextLabel = appMetricsContextLabel;
                    })
                .Report
                .ToInfluxDb(
                    options =>
                    {
                        var filter = new MetricsFilter();
                        filter.WhereContext(c => c == appMetricsContextLabel); //remove default AppMetrics metrics.
                        options.InfluxDb.BaseUri = new Uri($"http://{ Configuration["InfluxDbConfig.Host"] }:{ Configuration["InfluxDbConfig.Port"] }");
                        options.InfluxDb.Database = Configuration["InfluxDbConfig.Database"];
                        options.InfluxDb.RetentionPolicy = Configuration["InfluxDbConfig.RetentionPolicy"];
                        options.InfluxDb.UserName = Configuration["InfluxDbConfig.Username"];
                        options.InfluxDb.Password = Configuration["InfluxDbConfig.Password"];
                        options.InfluxDb.CreateDataBaseIfNotExists = false;
                        options.MetricsOutputFormatter = new MetricsInfluxDbLineProtocolOutputFormatter(
                            new MetricsInfluxDbLineProtocolOptions
                            {
                                MetricNameFormatter = (metricContext, metricName) =>
                                    string.IsNullOrWhiteSpace(metricContext)
                                        ? $"{metricName}".Replace(' ', '_')
                                        : $"{metricContext}_{metricName}".Replace(' ', '_')
                            });
                    })
                .Build();

            services.AddSingleton<IFusionCache>(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<ZiggyCreatures.Caching.Fusion.FusionCache>>();

                var fusionCacheOptions = new FusionCacheOptions
                {
                    DefaultEntryOptions = new FusionCacheEntryOptions
                        {
                            Duration = TimeSpan.FromSeconds(5),
                            Priority = CacheItemPriority.High
                        }
                        .SetFailSafe(true, TimeSpan.FromSeconds(10))
                        .SetFactoryTimeouts(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(10))
                };

                // Future Plugin for hooking metrics ???
                var metrics = new AppMetricsProvider("domain", appMetrics);
                var fusionCache = new ZiggyCreatures.Caching.Fusion.FusionCache(fusionCacheOptions, hostNameCache, logger);
                metrics.Wireup(fusionCache, fusionCacheOptions);

                return fusionCache;
            });

            services.AddSingleton(new DataManager());

            services.AddSingleton<IEmailService>(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<ZiggyCreatures.Caching.Fusion.FusionCache>>();

                var fusionCacheOptions = new FusionCacheOptions
                {
                    DefaultEntryOptions = new FusionCacheEntryOptions
                        {
                            Duration = TimeSpan.FromSeconds(5),
                            Priority = CacheItemPriority.High
                        }
                        .SetFailSafe(true, TimeSpan.FromSeconds(10))
                        .SetFactoryTimeouts(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(10))
                };

                var metrics = new AppMetricsProvider("email", appMetrics);
                var fusionCache = new ZiggyCreatures.Caching.Fusion.FusionCache(fusionCacheOptions, hostNameCache, logger);
                metrics.Wireup(fusionCache, fusionCacheOptions);

                return new EmailService(serviceProvider.GetRequiredService<DataManager>(), fusionCache);
            });


            var metricsReporterService = new MetricsReporterBackgroundService(appMetrics, appMetrics.Options, appMetrics.Reporters);
            metricsReporterService.StartAsync(CancellationToken.None);

            services.AddSingleton(sp => metricsReporterService );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
