using Azure.Identity;
using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.PeriodicBatching;

namespace CB.Serilog.Sinks.AzureLogAnalytics.Configuration;

public static class AzureLogAnalyticsSinkConfigurationExtensions
{
public static LoggerConfiguration AzureLogAnalytics(
    this LoggerSinkConfiguration loggerConfiguration,
    string ruleId,
    string streamName,
    Uri dataCollectionEndpointUri,
    bool outputToConsole = false,
    int maxLogEntries = 100, // Matches your "MaxLogEntries" in JSON
    TimeSpan? period = null,
    IFormatProvider? formatProvider = null)
{
    // 1. Map individual arguments to your config object
    var config = new AzureLogAnalyticsSinkConfiguration
    {
        RuleId = ruleId,
        StreamName = streamName,
        DataCollectionEndpointUri = dataCollectionEndpointUri,
        OutputToConsole = outputToConsole,
        BatchSizeLimit = maxLogEntries,
        Period = period ?? TimeSpan.FromSeconds(2)
    };

    // 2. Initialize the sink as we did before
    var azureSink = new AzureLogAnalyticsSink(config);
    var batchingOptions = new PeriodicBatchingSinkOptions
    {
        BatchSizeLimit = config.BatchSizeLimit,
        Period = config.Period,
        QueueLimit = config.QueueLimit
    };

    var batchingSink = new PeriodicBatchingSink(azureSink, batchingOptions);
    return loggerConfiguration.Sink(batchingSink);
}
        
    public static LoggerConfiguration AzureLogAnalytics(
        this LoggerSinkConfiguration loggerConfiguration,
        Func<AzureLogAnalyticsSinkConfiguration> configure, IFormatProvider? formatProvider = null
        )
    {
        var azureConfiguration = configure();
        return loggerConfiguration.AzureLogAnalytics(azureConfiguration, formatProvider);
    }
}
