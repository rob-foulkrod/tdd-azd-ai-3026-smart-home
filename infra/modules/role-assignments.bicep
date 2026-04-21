targetScope = 'resourceGroup'

@description('Name of the AI Services account for scoping role assignments.')
param aiServicesName string

@description('Name of the Web App to read its managed identity principal ID.')
param webAppName string

@description('Principal ID of the deployer (empty string skips deployer assignments).')
param principalId string = ''

// Reference existing resources to get the correct principal IDs at deployment time
resource aiServices 'Microsoft.CognitiveServices/accounts@2025-06-01' existing = {
  name: aiServicesName
}

resource webApp 'Microsoft.Web/sites@2024-04-01' existing = {
  name: webAppName
}

// Role definition IDs
var cognitiveServicesOpenAiContributorRoleId = 'a001fd3d-188f-4b5d-821b-7da978bf7442'
var azureAiDeveloperRoleId = '64702f94-c441-49e6-a78b-ef80e0188fee'
var azureAiUserRoleId = '53ca6127-db72-4b80-b1b0-d745d6d5456d'

// Web App MI — Cognitive Services OpenAI Contributor (scoped to AI Services account)
resource webAppOpenAiContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aiServices.id, webApp.name, cognitiveServicesOpenAiContributorRoleId)
  scope: aiServices
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', cognitiveServicesOpenAiContributorRoleId)
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Web App MI — Azure AI Developer (scoped to AI Services account, for Foundry Agent Service)
resource webAppAiDeveloperRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aiServices.id, webApp.name, azureAiDeveloperRoleId)
  scope: aiServices
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', azureAiDeveloperRoleId)
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Web App MI — Azure AI User (scoped to AI Services account, required for agents/read data action)
resource webAppAiUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aiServices.id, webApp.name, azureAiUserRoleId)
  scope: aiServices
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', azureAiUserRoleId)
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Deployer — Cognitive Services OpenAI Contributor (scoped to AI Services account)
resource deployerOpenAiContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(principalId)) {
  name: guid(aiServices.id, principalId, cognitiveServicesOpenAiContributorRoleId)
  scope: aiServices
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', cognitiveServicesOpenAiContributorRoleId)
    principalId: principalId
    principalType: 'User'
  }
}

// Deployer — Azure AI Developer (scoped to AI Services account)
resource deployerAiDeveloperRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(principalId)) {
  name: guid(aiServices.id, principalId, azureAiDeveloperRoleId)
  scope: aiServices
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', azureAiDeveloperRoleId)
    principalId: principalId
    principalType: 'User'
  }
}

// Deployer — Azure AI User (scoped to AI Services account)
resource deployerAiUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(principalId)) {
  name: guid(aiServices.id, principalId, azureAiUserRoleId)
  scope: aiServices
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', azureAiUserRoleId)
    principalId: principalId
    principalType: 'User'
  }
}
