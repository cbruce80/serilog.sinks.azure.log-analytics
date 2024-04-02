using Azure.Core;
using Serilog.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.AzureLogAnalytics
{
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
            var configuration = configure();

            return loggerConfiguration.Sink(new AzureLogAnalyticsSink(formatProvider, configuration));
        }
    }
}
