using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Ingestion;
using CB.Serilog.Sinks.AzureLogAnalytics.Configuration;
using Serilog.Debugging;
using Serilog.Events;
using System.Text.Json;
using IBatchedLogEventSink = Serilog.Sinks.PeriodicBatching.IBatchedLogEventSink;

namespace CB.Serilog.Sinks.AzureLogAnalytics;

/// <summary>
/// A Serilog sink that targets Azure Log Analytics via the Ingestion API
/// </summary>
public class AzureLogAnalyticsSink : IBatchedLogEventSink
{
    private readonly LogsIngestionClient _logIngestionClient;
    private readonly TokenCredential _tokenCredential;
    private readonly Func<LogEvent, IDictionary<string, object>> _transform;

    private readonly AzureLogAnalyticsSinkConfiguration _config;
    private readonly IFormatProvider? _formatProvider;
    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="formatProvider"></param>
    /// <param name="configuration"></param>
    public AzureLogAnalyticsSink(
        AzureLogAnalyticsSinkConfiguration config,
        IFormatProvider? formatProvider = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _formatProvider = formatProvider;

        if (_config.DataCollectionEndpointUri == null)
            throw new ArgumentException("DataCollectionEndpointUri must be provided.");

        if (string.IsNullOrWhiteSpace(_config.RuleId))
            throw new ArgumentException("RuleId must be provided.");

        if (string.IsNullOrWhiteSpace(_config.StreamName))
            throw new ArgumentException("StreamName must be provided.");

        if (_config.TokenCredential != null)
        {
            _tokenCredential = _config.TokenCredential;
        }
        else
        {
            _tokenCredential = new DefaultAzureCredential();
        }
        if (_config.Transform != null)
        {
            _transform = _config.Transform;
        }
        else
        {
            _transform = transform;
        }
        _logIngestionClient = new LogsIngestionClient(
            _config.DataCollectionEndpointUri, _tokenCredential);
    }

    /// <summary>
    /// Writes a batch of log events to the Log Analytics Ingestion API
    /// </summary>
    /// <param name="batch"></param>
    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        var logItems = new List<IDictionary<string, object>>();

        foreach (var logEvent in batch)
        {
            var logObject = _transform(logEvent);

            if (_config.OutputToConsole)
            {
                var json = JsonSerializer.Serialize(logObject, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(json);
            }

            logItems.Add(logObject);
        }

        if (logItems.Count == 0)
        {
            return;
        }

        try
        {
            var jsonLog = JsonSerializer.Serialize(logItems, new JsonSerializerOptions { WriteIndented = true });
            var response = await _logIngestionClient.UploadAsync(_config.RuleId, _config.StreamName, RequestContent.Create(jsonLog));

            if (response.IsError)
            {
                SelfLog.WriteLine("AzureLogAnalyticsSink: Error posting to ingestion api: {0} {1}", response.Status, response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine("AzureLogAnalyticsSink: {0} StackTrace: {1}", ex.Message, ex.StackTrace);
        }
    }


    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    private IDictionary<string, object> transform(LogEvent logEvent)
    {
        var properties = new Dictionary<string, object?>();
        foreach (var lep in logEvent.Properties)
        {
            properties[lep.Key] = GetValue(lep.Value);
        }

        Dictionary<string, object>? exDic = null;
        if (logEvent.Exception != null)
        {
            exDic = new Dictionary<string, object>
            {
                { "Message", logEvent.Exception.Message.Replace("{", "{{").Replace("}", "}}") },
                { "StackTrace", logEvent.Exception.StackTrace ?? string.Empty }
            };
        }

        var logObject = new Dictionary<string, object>
        {
            { "TimeGenerated", logEvent.Timestamp },
            { "Level", logEvent.Level.ToString() },
            { "Template", logEvent.MessageTemplate.Text },
            { "Message", logEvent.RenderMessage(_formatProvider) },
            { "Properties", properties }
        };

        if (exDic != null)
        {
            logObject["Exception"] = exDic;
        }

        if (properties.ContainsKey("SourceContext"))
        {
            var logger = properties["SourceContext"];
            properties.Remove("SourceContext");
            logObject["Logger"] = logger ?? string.Empty;
        }

        return logObject;
    }

    private static object? GetValue(LogEventPropertyValue value)
    {
        if (value is ScalarValue scalarValue)
        {
            return scalarValue.Value;
        }

        if (value is SequenceValue sequenceValue)
        {
            return sequenceValue.Elements.Select(GetValue).ToArray();
        }

        if (value is StructureValue structureValue)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var property in structureValue.Properties)
            {
                dict[property.Name] = GetValue(property.Value);
            }
            return dict;
        }

        if (value is DictionaryValue dictionaryValue)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var kvp in dictionaryValue.Elements)
            {
                var key = GetValue(kvp.Key)?.ToString() ?? string.Empty;
                dict[key] = GetValue(kvp.Value);
            }
            return dict;
        }

        return value.ToString();
    }
}
