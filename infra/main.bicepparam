using './main.bicep'

param location = readEnvironmentVariable('AZURE_LOCATION', 'swedencentral')
param environment = readEnvironmentVariable('AZURE_ENV_NAME', 'dev')
param principalId = readEnvironmentVariable('AZURE_PRINCIPAL_ID', '')
