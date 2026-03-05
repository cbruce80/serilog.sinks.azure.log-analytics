param environment string

var logAnalyticsCustomTable = 'TestLogs_CL'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'cb-analytics-${environment}'
  location: resourceGroup().location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource customTable 'Microsoft.OperationalInsights/workspaces/tables@2022-10-01' = {
  parent: logAnalyticsWorkspace
  name: logAnalyticsCustomTable
  properties: {
    retentionInDays: 30
    totalRetentionInDays: 30
    schema: {
      name: logAnalyticsCustomTable
      columns: [
        {
          name: 'Exception'
          type: 'dynamic'
          description: 'Any Exception generated'
        }
        {
          name: 'Level'
          type: 'string'
          description: 'Log level'
        }
        {
          name: 'Logger'
          type: 'string'
          description: 'Logger name'
        }
        {
          name: 'Template'
          type: 'string'
          description: 'Logger Template'
        }
        {
          name: 'Message'
          type: 'string'
          description: 'Log message'
        }
        {
          name: 'Properties'
          type: 'dynamic'
          description: 'Logger properties'
        }
        {
          name: 'TimeGenerated'
          type: 'datetime'
          description: 'Time generated'
        }
      ]
    }
  }
}

resource dataCollectionEndpoint 'Microsoft.Insights/dataCollectionEndpoints@2022-06-01' = {
  name: 'cb-dce-${environment}'
  location: resourceGroup().location
  properties: {
    description: 'cb api logs ingestion'
    networkAcls: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

resource dataCollectionRule 'Microsoft.Insights/dataCollectionRules@2022-06-01' = {
  name: 'cb-dcr-${environment}'
  location: resourceGroup().location
  tags: {
    env: environment
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    description: 'data collection rule for test logs'
    dataCollectionEndpointId: dataCollectionEndpoint.id
    streamDeclarations: {
      'Custom-${logAnalyticsCustomTable}': {
        columns: [
          {
            name: 'TimeGenerated'
            type: 'datetime'
          }
          {
            name: 'Level'
            type: 'string'
          }
          {
            name: 'Logger'
            type: 'string'
          }
          {
            name: 'Template'
            type: 'string'
          }
          {
            name: 'Message'
            type: 'string'
          }
          {
            name: 'Exception'
            type: 'dynamic'
          }
          {
            name: 'Properties'
            type: 'dynamic'
          }
        ]
      }
    }
    destinations: {
      logAnalytics: [
        {
          name: logAnalyticsCustomTable
          workspaceResourceId: logAnalyticsWorkspace.id
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Custom-${logAnalyticsCustomTable}'
        ]
        destinations: [
          logAnalyticsCustomTable
        ]
        transformKql: 'source'
        outputStream: 'Custom-${logAnalyticsCustomTable}'
      }
    ]
  }
  dependsOn: [
    customTable
  ]
}

output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output dataCollectionEndpoint string = dataCollectionEndpoint.properties.logsIngestion.endpoint
output ruleId string = dataCollectionRule.properties.immutableId
