# Serilog Sink for Azure Log Analytics 
A bare bones custom [Serilog](https://serilog.net/) sink for Azure Log Analytics. Supports batching of logs. Utilizes the `Azure.Monitor.Ingestion` library for ingestion to Log Analytics. 
## Prerequisites
1. Azure Subscription
2. Log Analytics workspace provisioned
3. A configured [data collection endpoint and data collection rule](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/tutorial-logs-ingestion-portal) in Azure with appropriate permissions.
## Get Started

### Install package
```dotnetcli
dotnet add package Serilog.AzureLogAnalytics
```
### Authenticate with Azure
By default, a [DefaultAzureCredential](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/identity/Azure.Identity/README.md#defaultazurecredential) is used to authenticate with Azure and no additional code is needed. Optionally, a `TokenCredential` can be passed in for use.
### Configure
The sink can be configured programatticaly or through Serilog `Serilog.Settings.Configuration` NuGet package. An instance of `AzureLogAnalyticsSinkConfiguration` is required.

####
App Config Example
`program.cs`
```C# Snippet:AppConfig
Host.CreateDefaultBuilder()
    .UseSerilog((hostingContext, services, loggerConfiguration) =>
    {
        var assemblies = new[] { typeof(AzureLogAnalyticsSink).Assembly };
        var options = new ConfigurationReaderOptions(assemblies);
        loggerConfiguration
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(hostingContext.Configuration, options);
    });

```
`JSON:app.config.json`
```
 "Serilog": {
    "Using": [ "Serilog.Sinks.AzureLogAnalytics" ],
    "MinimumLevel": "Verbose",
    "WriteTo": [
      {
        "Name": "AzureLogAnalytics",
        "Args": {
          "DataCollectionEndpointUri": "<data-collection-endpoint>",
          "RuleId": "<rule-id>",
          "StreamName": "<stream-name>",
          "OutputToConsole": true,
          "MaxLogEntries": 5
        }
      }
    ]
  }
```
#### IHostBuilder Example
`program.cs`
```C# Snippet:HostBuilder
Host.CreateDefaultBuilder()
    .UseSerilog((hostingContext, services, loggerConfiguration) =>
    {
        loggerConfiguration
        .Enrich.FromLogContext()
        .WriteTo.AzureLogAnalytics(new AzureLogAnalyticsSinkConfiguration
        {
            DataCollectionEndpointUri = new Uri("<data-collection-endpoint>"),
            MaxLogEntries = 10,
            RuleId = "<rule-id>",
            StreamName = "<stream-name>",
            OutputToConsole = true,
            TokenCredential = "optional token credential"
        })
    });

```
The `AzureLogAnalyticsSinkConfiguration`  type is used to configure the sink. 

|Property | Description
|-|-
DataCollectionEndpointUri|The data collection endpoindt setup in Azure
MaxLogEntries|The maximum number of log entries to buffer before flushing to Log Analytics
RuleId|The data collection rule id setup in Azure
StreamName|The data collection rule stream name (your custom log analytics table
OutputToConsole|An optional feature that outputs log output to the console. This is useful when configuring your data collection rule schema and transformation.
TokenCredential|An instance of TokenCredential used to authenticate with Azure. This is optional. By default an DefaultAzureCredential is used.
Transform|An optional Func<LogEvent, IDictionary<string, object>> to transform a Serilog LogEvent to an IDictionary<string, object> that will be serialized to Json to be sent to Log Analytics. A default implementation is provided if not set. **Note:** *This method should be thread-safe, as it will be called concurrently by Serilog.*

