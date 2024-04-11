using Azure.Core.Diagnostics;
using Azure.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Debugging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CB.Serilog.Sinks.AzureLogAnalytics.Sample;

public class TestLoggingService : IHostedService
{
    private readonly ILogger<TestLoggingService> _logger;
    public TestLoggingService(ILogger<TestLoggingService> logger)
    {
        _logger = logger;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();
        /*
        SelfLog.Enable(msg => Console.WriteLine(msg));
        using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();
        DefaultAzureCredentialOptions options = new DefaultAzureCredentialOptions
        {
            Diagnostics =
            {
                LoggedHeaderNames = { "x-ms-request-id" },
                LoggedQueryParameters = { "api-version" },
                IsLoggingContentEnabled = true,
                IsAccountIdentifierLoggingEnabled = true
            }
        };
        */
        _logger.LogInformation("Info {value}", "test value");
        _logger.LogDebug("Debug {debugVal}", "debug");
        _logger.LogError("error");
        try
        {
            throw new DivideByZeroException("error dividing by 0");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception error");
        }


        for (int i = 0; i <= 50; i++)
        {
            _logger.LogInformation("Test Log #{num}", i);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

