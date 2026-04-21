targetScope = 'resourceGroup'

@description('Azure region for the Web App.')
param location string

@description('Web App name.')
param name string

@description('Resource tags.')
param tags object

@description('Resource ID of the App Service Plan.')
param planId string

@description('Foundry Project endpoint URL.')
param projectEndpoint string

@description('Application Insights resource ID.')
param appInsightsResourceId string

module site 'br/public:avm/res/web/site:0.22.0' = {
  name: '${name}-deploy'
  params: {
    name: name
    location: location
    tags: union(tags, { 'azd-service-name': 'web' })
    kind: 'app,linux'
    serverFarmResourceId: planId
    managedIdentities: {
      systemAssigned: true
    }
    configs: [
      {
        name: 'appsettings'
        applicationInsightResourceId: appInsightsResourceId
        properties: {
          PROJECT_ENDPOINT: projectEndpoint
        }
      }
    ]
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
  }
}

output siteId string = site.outputs.resourceId
output siteName string = site.outputs.name
output siteDefaultHostname string = site.outputs.defaultHostname
output systemAssignedPrincipalId string = site.outputs.systemAssignedMIPrincipalId ?? ''
