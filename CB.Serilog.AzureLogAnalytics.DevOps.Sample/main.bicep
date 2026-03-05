targetScope = 'subscription' // This allows the creation of a Resource Group

param location string
param environment string
param rgName string = 'rg-monitoring-${environment}'

//Create the Resource Group
resource newRG 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: rgName
  location: location
}

// Deploy the Monitoring Resources into that Resource Group
module monitoringResources 'monitoring.bicep' = {
  scope: newRG // This tells Bicep to switch context to the new RG
  name: 'monitoringDeployment'
  params: {
    environment: environment
  }
}

// Outputs need to reference the module now
output logAnalyticsWorkspaceId string = monitoringResources.outputs.logAnalyticsWorkspaceId
output dataCollectionEndpoint string = monitoringResources.outputs.dataCollectionEndpoint
output ruleId string = monitoringResources.outputs.ruleId
