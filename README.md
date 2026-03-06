# Serilog Sink for Azure Log Analytics

[![NuGet Version](https://img.shields.io/nuget/v/CB.Serilog.Sinks.AzureLogAnalytics.svg)](https://www.nuget.org/packages/CB.Serilog.Sinks.AzureLogAnalytics/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A bare-bones custom [Serilog](https://serilog.net/) sink for Azure Log Analytics. Supports batching of logs and utilizes the `Azure.Monitor.Ingestion` library for ingestion to Log Analytics via the new Log Ingestion API.

## Prerequisites

1. Azure Subscription
2. Log Analytics workspace provisioned
3. A configured [data collection endpoint and data collection rule](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/tutorial-logs-ingestion-portal) in Azure with appropriate permissions.

## Getting Started

### Install Package

```bash
dotnet add package CB.Serilog.Sinks.AzureLogAnalytics
```

### Authenticate with Azure

By default, a [DefaultAzureCredential](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/identity/Azure.Identity/README.md#defaultazurecredential) is used to authenticate with Azure, meaning no additional code is needed if your environment is already configured. Optionally, a custom `TokenCredential` can be passed in during configuration.

### Configure

The sink can be configured programmatically or through the `Serilog.Settings.Configuration` NuGet package via `appsettings.json`. An instance of `AzureLogAnalyticsSinkConfiguration` is required.

#### App Config Example

`Program.cs`
```csharp
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

`appsettings.json`
```json
{
  "Serilog": {
    "Using": [ "CB.Serilog.Sinks.AzureLogAnalytics" ],
    "MinimumLevel": "Verbose",
    "WriteTo": [
      {
        "Name": "AzureLogAnalytics",
        "Args": {
          "DataCollectionEndpointUri": "https://<data-collection-endpoint>.logs1.azure.com",
          "RuleId": "dcr-<rule-id>",
          "StreamName": "Custom-MyLogs_CL",
          "OutputToConsole": true,
          "MaxLogEntries": 5
        }
      }
    ]
  }
}
```

#### IHostBuilder Example

`Program.cs`
```csharp
Host.CreateDefaultBuilder()
    .UseSerilog((hostingContext, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .Enrich.FromLogContext()
            .WriteTo.AzureLogAnalytics(new AzureLogAnalyticsSinkConfiguration
            {
                DataCollectionEndpointUri = new Uri("https://<data-collection-endpoint>.logs1.azure.com"),
                RuleId = "dcr-<rule-id>",
                StreamName = "Custom-MyLogs_CL",
                MaxLogEntries = 10,
                OutputToConsole = true
                // TokenCredential = new DefaultAzureCredential() // Optional
            });
    });
```

### Configuration Options

The `AzureLogAnalyticsSinkConfiguration` type is used to configure the sink.

| Property | Description |
|---|---|
| **DataCollectionEndpointUri** | The Data Collection Endpoint URI set up in Azure. |
| **MaxLogEntries** | The maximum number of log entries to buffer before flushing to Log Analytics. |
| **RuleId** | The Data Collection Rule ID set up in Azure. |
| **StreamName** | The Data Collection Rule stream name (e.g., your custom Log Analytics table like `Custom-MyLogs_CL`). |
| **OutputToConsole** | An optional feature that outputs payload format to the console. This is incredibly useful when configuring and troubleshooting your Data Collection Rule schema and transformations. |
| **TokenCredential** | An instance of `TokenCredential` used to authenticate with Azure. This is optional; by default, a `DefaultAzureCredential` is used. |
| **Transform** | An optional `Func<LogEvent, IDictionary<string, object>>` to transform a Serilog `LogEvent` to an `IDictionary<string, object>` that will be serialized to JSON and sent to Log Analytics. A default implementation is provided if not set. **Note:** *This method should be thread-safe, as it will be called concurrently by Serilog.* |

## Default Log Schema

By default, if you don't provide a custom `Transform` function, the sink maps Serilog properties to the following JSON structure which you will need to map your Data Collection Rule schema to:

```json
{
  "Timestamp": "2023-10-01T12:00:00.0000000Z",
  "Level": "Information",
  "Message": "This is a log message",
  "Exception": "Exception details if any",
  "Properties": {
    "CustomProperty": "CustomValue"
  }
}
```
You should define your custom Log Analytics table columns appropriately to match the default transformation output, or provide your own `Transform` function if your schema dictates a different structure.

## Infrastructure as Code (Azure Setup)

Configuring the Log Ingestion API requires setting up a Log Analytics Workspace, a Data Collection Endpoint (DCE), a Data Collection Rule (DCR), and granting `Monitoring Metrics Publisher` permissions via RBAC to the managed identity (or app registration) that will be sending logs.

We have provided a reference implementation to automate this infrastructure in the `CB.Serilog.AzureLogAnalytics.DevOps.Sample` directory using **Azure Bicep**.

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue if you encounter any problems or have feature suggestions.

## License

This project is licensed under the MIT License - see the `LICENSE` file for details.