@description('Environment name(dev, staging, prod)')
param environment string = 'dev'

@description('Azure region for the resources')
param location string = resourceGroup().location

@description('unique suffix for resource names')
param uniqueSuffix string = uniqueString(resourceGroup().id)

var functionAppName = 'functionapp-${environment}-${uniqueSuffix}'
var storageAccountName = 'storagetax${environment}${uniqueSuffix}'
var appInsightsName = 'ai-tax-processor-${environment}'
var appServicePlanName = 'asp-tax-processor-${environment}-${uniqueSuffix}'

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: substring(storageAccountName, 0, min(length(storageAccountName), 24))
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}
