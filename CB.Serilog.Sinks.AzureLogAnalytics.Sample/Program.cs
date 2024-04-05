// See https://aka.ms/new-console-template for more information

using CB.Serilog.Sinks.AzureLogAnalytics;
using CB.Serilog.Sinks.AzureLogAnalytics.Sample;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole;
using Serilog.Debugging;
using Serilog.Settings.Configuration;
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
        SelfLog.Enable(msg => Console.WriteLine(msg));
        var assemblies = new[] {
            typeof(AzureLogAnalyticsSink).Assembly,
            typeof(Serilog.Sinks.SystemConsole.Themes.ConsoleTheme).Assembly
        };
        var options = new ConfigurationReaderOptions(assemblies);
        loggerConfiguration
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(hostingContext.Configuration, options);
    });


await host.RunConsoleAsync();
