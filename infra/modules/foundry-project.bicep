targetScope = 'resourceGroup'

@description('Azure region for the Foundry project.')
param location string

@description('Resource tags.')
param tags object

@description('Name of the parent AI Services account.')
param aiServicesAccountName string

@description('Project name.')
param projectName string

resource aiServices 'Microsoft.CognitiveServices/accounts@2025-06-01' existing = {
  name: aiServicesAccountName
}

resource project 'Microsoft.CognitiveServices/accounts/projects@2025-06-01' = {
  parent: aiServices
  name: projectName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {}
}

output projectName string = project.name
output projectEndpoint string = 'https://${aiServices.name}.services.ai.azure.com/api/projects/${project.name}'
output projectResourceId string = project.id
