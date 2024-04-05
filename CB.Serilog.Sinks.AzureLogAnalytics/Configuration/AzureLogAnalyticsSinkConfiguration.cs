using Azure.Core;
using Serilog.Events;

namespace CB.Serilog.Sinks.AzureLogAnalytics.Configuration
{
    /// <summary>
    /// Configuration for AzureLogAnalyticsSink
    /// </summary>
    public class AzureLogAnalyticsSinkConfiguration
    {
        /// <summary>
        /// In addtion to flushing to the Data Ingestion API, logs are output to the console. Useful for extracting JSON schema when building an Azure Data Collection Rule (DCR)
        /// </summary>
        public bool OutputToConsole { get; set; }
        /// <summary>
        /// The maximum number of log entries buffered before they are flushed to the Ingestion API
        /// </summary>
        public int MaxLogEntries { get; set; } = 10;
        /// <summary>
        /// Data Collection Rule - immutableId
        /// </summary>
        public string RuleId { get; set; } = string.Empty;
        /// <summary>
        /// Data Collection Rile - steamDeclarations
        /// </summary>
        public string StreamName { get; set; } = string.Empty;
        /// <summary>
        /// Data Collection Endpoint Ingestion uri
        /// </summary>
        public Uri? DataCollectionEndpointUri { get; set; }
        /// <summary>
        /// Azure Token Credential to use to authenticate to data collection rule. A <see cref="DefaultAzureCredential"/> will be used if not provided
        /// </summary>
        public TokenCredential? TokenCredential { get; set; }
        /// <summary>
        /// Allows the customization of the JSON sent to Log Analytics
        /// </summary>
        public Func<LogEvent, IDictionary<string, object>>? Transform { get; set; }
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public AzureLogAnalyticsSinkConfiguration()
        {
        }
    }
}
