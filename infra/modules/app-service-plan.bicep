targetScope = 'resourceGroup'

@description('Azure region for the App Service Plan.')
param location string

@description('App Service Plan name.')
param name string

@description('Resource tags.')
param tags object

module plan 'br/public:avm/res/web/serverfarm:0.7.0' = {
  name: '${name}-deploy'
  params: {
    name: name
    location: location
    tags: tags
    skuName: 'P1v3'
    skuCapacity: 1
    kind: 'linux'
    reserved: true
    zoneRedundant: false
  }
}

output planId string = plan.outputs.resourceId
output planName string = plan.outputs.name
