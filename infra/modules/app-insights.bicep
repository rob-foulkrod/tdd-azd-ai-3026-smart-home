targetScope = 'resourceGroup'

@description('Azure region for Application Insights.')
param location string

@description('Application Insights resource name.')
param name string

@description('Resource tags.')
param tags object

@description('Log Analytics workspace resource ID.')
param workspaceId string

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  kind: 'web'
  tags: tags
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspaceId
  }
}

output resourceId string = appInsights.id
output connectionString string = appInsights.properties.ConnectionString
output instrumentationKey string = appInsights.properties.InstrumentationKey
