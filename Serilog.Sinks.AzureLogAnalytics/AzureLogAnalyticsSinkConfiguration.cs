using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.AzureLogAnalytics
{
    public class AzureLogAnalyticsSinkConfiguration
    {
        /// <summary>
        /// Outputs logs to console. Useful to extract JSON schema when building a Azure Data Collection Rule (DCR)
        /// </summary>
        public bool InputToConsole { get; set; }
    }
}
