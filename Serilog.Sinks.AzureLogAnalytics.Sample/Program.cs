﻿// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Settings.Configuration;
using Serilog.Sinks.AzureLogAnalytics;
using Serilog.Sinks.AzureLogAnalytics.Sample;
using System.Data;
using System.Reflection;

var host = Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration(builder =>
    {
        builder.AddUserSecrets(Assembly.GetExecutingAssembly());
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<TestLoggingService>();

    })
    .UseSerilog((hostingContext, services, loggerConfiguration) =>
    {
        var assemblies = new[] { typeof(AzureLogAnalyticsSink).Assembly };
        var options = new ConfigurationReaderOptions(assemblies);
        loggerConfiguration
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(hostingContext.Configuration, options);
    });


await host.RunConsoleAsync();
