using Azure.Identity;
using Azure.Monitor.Ingestion;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.AzureLogAnalytics;

/// <summary>
/// A Serilog sink that targets Azure Log Analytics via the Ingestion API
/// </summary>
public class AzureLogAnalyticsSink : ILogEventSink
{
    private readonly IFormatProvider? _formatProvider;
    private readonly LogsIngestionClient _logIngestionClient;
    private readonly AzureLogAnalyticsSinkConfiguration _configuration;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="formatProvider"></param>
    /// <param name="configuration"></param>
    public AzureLogAnalyticsSink(IFormatProvider? formatProvider, AzureLogAnalyticsSinkConfiguration configuration)
    {
        _configuration = configuration;
        _formatProvider = formatProvider;
    }
    public void Emit(LogEvent logEvent)
    {
 
        var tokenCredential = new DefaultAzureCredential();
        var endpoint = new Uri("<data_collection_endpoint>");
        var ruleId = "<data_collection_rule_id>";
        var streamName = "<stream_name>";

        var client = new LogsIngestionClient(endpoint, tokenCredential);
    }
}

