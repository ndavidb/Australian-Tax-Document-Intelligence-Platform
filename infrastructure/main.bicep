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
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

// Blob Containers
resource receiptsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/receipts'
  properties: {
    publicAccess: 'None'
  }
}

resource processedContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/processed'
  properties: {
    publicAccess: 'None'
  }
}

resource summariesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/summaries'
  properties: {
    publicAccess: 'None'
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  sku: {
      name: 'Y1'
      tier: 'Dynamic'
    }
    properties: {
      reserved: true  // For Linux
    }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
       appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'StorageConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
      ]
    netFrameworkVersion: 'v8.0'
    }
  }
}

output functionAppName string = functionApp.name
output storageAccountName string = storageAccount.name
output appInsightsName string = appInsights.name
