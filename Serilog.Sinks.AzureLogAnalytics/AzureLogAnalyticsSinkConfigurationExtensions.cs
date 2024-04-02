using Serilog.Configuration;

namespace Serilog.Sinks.AzureLogAnalytics;

public static class AzureLogAnalyticsSinkConfigurationExtensions
{
    public static LoggerConfiguration AzureLogAnalytics(
          this LoggerSinkConfiguration loggerConfiguration,
          AzureLogAnalyticsSinkConfiguration azureConfiguration, IFormatProvider? formatProvider = null)
    {
        return loggerConfiguration.Sink(new AzureLogAnalyticsSink(formatProvider, azureConfiguration));
    }

    public static LoggerConfiguration AzureLogAnalytics(
        this LoggerSinkConfiguration loggerConfiguration,
        Func<AzureLogAnalyticsSinkConfiguration> configure, IFormatProvider? formatProvider = null
        )
    {

        var azureConfiguration = configure();

        return loggerConfiguration.Sink(new AzureLogAnalyticsSink(formatProvider, azureConfiguration));
    }
}
