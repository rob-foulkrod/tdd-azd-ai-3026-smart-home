targetScope = 'subscription'

@description('Azure region for infrastructure (App Service, Log Analytics, App Insights).')
param location string = 'northcentralus'

@description('Azure region for AI Services and Foundry Project.')
param aiLocation string = 'eastus2'

@description('Environment name provided by azd (AZURE_ENV_NAME).')
param environment string

@description('Principal ID of the deployer (azd auto-provides via AZURE_PRINCIPAL_ID).')
param principalId string = ''

var projectName = 'ai3026sh'
var resourceGroupName = 'rg-${environment}'

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: {
    Environment: environment
    ManagedBy: 'Bicep'
    Project: projectName
    SecurityControl: 'Ignore'
  }
}

var uniqueSuffix = uniqueString(rg.id)

var tags = {
  Environment: environment
  ManagedBy: 'Bicep'
  Project: projectName
  SecurityControl: 'Ignore'
}

var logAnalyticsName = 'log-${environment}'
var aiServicesName = 'ai-${environment}-${take(uniqueSuffix, 6)}'
var appInsightsName = 'appi-${environment}'
var appServicePlanName = 'asp-${environment}'
var webAppName = 'app-${environment}'

module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'logAnalytics-deploy'
  scope: rg
  params: {
    name: logAnalyticsName
    location: location
    tags: tags
  }
}

module appInsights 'modules/app-insights.bicep' = {
  name: 'appInsights-deploy'
  scope: rg
  params: {
    name: appInsightsName
    location: location
    tags: tags
    workspaceId: logAnalytics.outputs.workspaceId
  }
}

module aiServices 'modules/ai-services.bicep' = {
  name: 'aiServices-deploy'
  scope: rg
  params: {
    name: aiServicesName
    location: aiLocation
    tags: tags
  }
}

var foundryProjectName = 'smarthome'

module foundryProject 'modules/foundry-project.bicep' = {
  name: 'foundryProject-deploy'
  scope: rg
  params: {
    aiServicesAccountName: aiServices.outputs.aiServicesName
    projectName: foundryProjectName
    location: aiLocation
    tags: tags
  }
}

module appServicePlan 'modules/app-service-plan.bicep' = {
  name: 'appServicePlan-deploy'
  scope: rg
  params: {
    name: appServicePlanName
    location: location
    tags: tags
  }
}

module webApp 'modules/web-app.bicep' = {
  name: 'webApp-deploy'
  scope: rg
  params: {
    name: webAppName
    location: location
    tags: tags
    planId: appServicePlan.outputs.planId
    projectEndpoint: foundryProject.outputs.projectEndpoint
    appInsightsResourceId: appInsights.outputs.resourceId
  }
}

module roleAssignments 'modules/role-assignments.bicep' = {
  name: 'roleAssignments-deploy'
  scope: rg
  params: {
    aiServicesName: aiServices.outputs.aiServicesName
    webAppName: webApp.outputs.siteName
    principalId: principalId
  }
}

output AZURE_RESOURCE_GROUP string = rg.name
output webAppName string = webApp.outputs.siteName
output webAppUrl string = 'https://${webApp.outputs.siteDefaultHostname}'
output projectEndpoint string = foundryProject.outputs.projectEndpoint
