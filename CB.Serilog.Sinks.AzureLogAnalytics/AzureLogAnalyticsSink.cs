using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Ingestion;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CB.Serilog.Sinks.AzureLogAnalytics;

/// <summary>
/// A Serilog sink that targets Azure Log Analytics via the Ingestion API
/// </summary>
public class AzureLogAnalyticsSink : ILogEventSink
{
    private readonly IFormatProvider? _formatProvider;
    private readonly LogsIngestionClient _logIngestionClient;
    private readonly AzureLogAnalyticsSinkConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly TokenCredential _tokenCredential;
    private ConcurrentBag<object> _InternalLogBuffer;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="formatProvider"></param>
    /// <param name="configuration"></param>
    internal AzureLogAnalyticsSink(IFormatProvider? formatProvider, AzureLogAnalyticsSinkConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        _configuration = configuration;

        if (_configuration.Transform is null)
        {
            _configuration.Transform = transform;
        }


        _formatProvider = formatProvider;
        _InternalLogBuffer = new ConcurrentBag<object>();

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            MaxDepth = 0,
            ReferenceHandler = ReferenceHandler.IgnoreCycles

        };

        if (configuration.TokenCredential != null)
        {
            _tokenCredential = configuration.TokenCredential;
        }
        else
        {
            _tokenCredential = new DefaultAzureCredential();
        }

        _logIngestionClient = new LogsIngestionClient(configuration.DataCollectionEndpointUri, _tokenCredential);
    }
    /// <summary>
    /// Writes an log event to an internal buffer and flushes to the Log Analytics Ingestion API if necessary
    /// </summary>
    /// <param name="logEvent"></param>
    public void Emit(LogEvent logEvent)
    {
        WriteEvent(logEvent);
    }
    private IDictionary<string, object> transform(LogEvent logEvent)
    {
        Dictionary<string, string> properties = new Dictionary<string, string>();
        foreach (var lep in logEvent.Properties)
        {
            if (logEvent.Properties.TryGetValue(lep.Key, out LogEventPropertyValue? value) && value is ScalarValue sv && sv.Value is string rawValue)
            {
                properties.Add(lep.Key, rawValue);
            }
        }

        var logObject = new ExpandoObject() as IDictionary<string, object>;
        logObject.Add("TimeGenerated", logEvent.Timestamp);
        logObject.Add("Level", logEvent.Level.ToString());

        logObject.Add("Template", logEvent.MessageTemplate.Text);
        logObject.Add("Message", logEvent.RenderMessage());
        logObject.Add("Exception", logEvent.Exception!);
        logObject.Add("Properties", properties);

        if (properties.ContainsKey("SourceContext"))
        {
            var logger = properties["SourceContext"];
            properties.Remove("SourceContext");
            logObject.Add("Logger", logger);
        }

        return logObject;
    }
    private void WriteEvent(LogEvent logEvent)
    {
        try
        {

            var logObject = _configuration.Transform!(logEvent);

            _InternalLogBuffer.Add(logObject);

            if (_configuration.OutputToConsole)
            {
                var json = JsonSerializer.Serialize(logObject, _jsonSerializerOptions);
                Console.WriteLine(json);
            }

            if (_InternalLogBuffer.Count >= _configuration.MaxLogEntries)
            {
                var jsonLog = JsonSerializer.Serialize(_InternalLogBuffer, _jsonSerializerOptions);
                var response = _logIngestionClient.Upload(_configuration.RuleId, _configuration.StreamName, RequestContent.Create(jsonLog));

                if (response.IsError)
                {
                    throw new Exception($"Error posting to ingestion api: {response.Status} {response.ReasonPhrase}");
                }
                else
                {
                    _InternalLogBuffer.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine($"AzureLogAnalyticsSink: {ex.Message}");
        }
    }
}

