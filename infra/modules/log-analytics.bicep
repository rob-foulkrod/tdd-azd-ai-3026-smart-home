targetScope = 'resourceGroup'

@description('Azure region for the workspace.')
param location string

@description('Log Analytics workspace name.')
param name string

@description('Resource tags.')
param tags object

module workspace 'br/public:avm/res/operational-insights/workspace:0.15.0' = {
  name: '${name}-deploy'
  params: {
    name: name
    location: location
    tags: tags
    dailyQuotaGb: '1'
  }
}

output workspaceId string = workspace.outputs.resourceId
output workspaceName string = workspace.outputs.name
