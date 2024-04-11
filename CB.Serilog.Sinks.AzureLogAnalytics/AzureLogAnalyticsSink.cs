using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Ingestion;
using CB.Serilog.Sinks.AzureLogAnalytics.Configuration;
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
public class AzureLogAnalyticsSink : ILogEventSink, IDisposable
{
    private readonly IFormatProvider? _formatProvider;
    private readonly LogsIngestionClient _logIngestionClient;
    private readonly AzureLogAnalyticsSinkConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly TokenCredential _tokenCredential;
    private readonly Func<LogEvent, IDictionary<string, object>> _transform;
    private readonly ConcurrentQueue<IDictionary<string, object>>_logEventQueue;
    //private ConcurrentBag<object> _InternalLogBuffer;
    private SemaphoreSlim _semaphore;
    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="formatProvider"></param>
    /// <param name="configuration"></param>
    internal AzureLogAnalyticsSink(IFormatProvider? formatProvider, AzureLogAnalyticsSinkConfiguration configuration)
    {
        _semaphore = new SemaphoreSlim(1);
        _transform = transform;

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        _configuration = configuration;

        if (  _configuration.Transform != null)
        {
            _transform = _configuration.Transform;
        }
     
        _formatProvider = formatProvider;
        _logEventQueue = new ConcurrentQueue<IDictionary<string, object>>();

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            MaxDepth = 0,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
        _jsonSerializerOptions.Converters.Add(new ExceptionConverter<Exception>());

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
        // transform
        var logObject = _transform(logEvent);

        if (_configuration.OutputToConsole)
        {
            var json = JsonSerializer.Serialize(logObject, _jsonSerializerOptions);
            Console.WriteLine(json);
        }

        _logEventQueue.Enqueue(logObject);

        if (_logEventQueue.Count >= _configuration.MaxLogEntries)
        {
            Task.Factory.StartNew(FlushEventsAsync, TaskCreationOptions.LongRunning);
        }
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
    private async Task FlushEventsAsync()
    {
            try
            {
                _semaphore.Wait();
                var logItems = new List<IDictionary<string, object>>();
                for (int i = 0; i <= _configuration.MaxLogEntries; i++)
                {
                    if (_logEventQueue.TryDequeue(out var item))
                    {
                        logItems.Add(item);
                    }
                }
                var jsonLog = JsonSerializer.Serialize(logItems, _jsonSerializerOptions);
                var response = await _logIngestionClient.UploadAsync(_configuration.RuleId, _configuration.StreamName, RequestContent.Create(jsonLog));

                if (response.IsError)
                {
                    SelfLog.WriteLine($"AzureLogAnalyticsSink: Error posting to ingestion api: {response.Status} {response.ReasonPhrase}");
                }

            }
            catch (Exception ex)
            {
                // santize any curly brackets so they aren't interpreted as format strings
                var sanitizedMessage = ex.Message?.Replace("{", "{{").Replace("}", "}}");
                SelfLog.WriteLine($"AzureLogAnalyticsSink: {sanitizedMessage}");
            }
            finally
            {
                Console.WriteLine("{0} release", Environment.CurrentManagedThreadId);
                _semaphore.Release();
            }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}

