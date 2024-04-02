# Serilog Sink for Azure Log Analytics 
A custom [Serilog](https://serilog.net/) sink for Azure Log Analytics. Supports batching of logs. Utilizes the underlying `Azure.Monitor.Ingestion` library for ingestion to Log Analytics.
## Prerequisites
1. Azure Subscription
2. Log Analytics workspace provisioned
3. A configured [data collection endpoint and data collection rule](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/tutorial-logs-ingestion-portal) in Azure with appropriate permissions.
## Get Started

### Install package
```dotnetcli
dotnet add package Azure.Identity
```
