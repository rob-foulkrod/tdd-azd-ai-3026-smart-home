targetScope = 'resourceGroup'

@description('Azure region for AI Services.')
param location string

@description('AI Services account name.')
param name string

@description('Resource tags.')
param tags object

resource aiServices 'Microsoft.CognitiveServices/accounts@2025-06-01' = {
  name: name
  location: location
  tags: tags
  kind: 'AIServices'
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: 'S0'
  }
  properties: {
    allowProjectManagement: true
    customSubDomainName: name
    disableLocalAuth: false
    publicNetworkAccess: 'Enabled'
  }
}

resource gpt4oDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-06-01' = {
  parent: aiServices
  name: 'gpt-4o'
  sku: {
    name: 'Standard'
    capacity: 30
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-11-20'
    }
  }
}

output aiServicesId string = aiServices.id
output aiServicesName string = aiServices.name
output openAiEndpoint string = 'https://${name}.openai.azure.com/'
