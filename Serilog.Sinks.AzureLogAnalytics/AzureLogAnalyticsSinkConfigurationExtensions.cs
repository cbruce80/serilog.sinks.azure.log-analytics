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
        public static LoggerConfiguration MySink(
              this LoggerSinkConfiguration loggerConfiguration,
              IFormatProvider? formatProvider = null)
        {
            return loggerConfiguration.Sink(new AzureLogAnalyticsSink(formatProvider));
        }
    }
}
