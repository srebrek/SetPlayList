targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name used to generate resource names (azd environment name).')
param environmentName string

@minLength(1)
@description('Azure region for all resources.')
param location string

@description('Login server of the shared ACR that hosts the app image, e.g. myregistry.azurecr.io.')
param acrLoginServer string

@description('Fully qualified container image, e.g. myregistry.azurecr.io/setplaylist:latest.')
param containerImageName string

@description('User-assigned managed identity resource id used to pull from ACR.')
param acrPullIdentityId string

@secure()
param spotifyClientId string

@secure()
param spotifyClientSecret string

@description('Public https URL the app is reachable at, e.g. https://setplaylist.example.com/signin-spotify.')
param spotifyRedirectUri string

@secure()
param setlistFmClientSecret string

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-11-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: {
    'azd-env-name': environmentName
  }
}

module resources 'resources.bicep' = {
  name: 'resources'
  scope: resourceGroup
  params: {
    environmentName: environmentName
    location: location
    acrLoginServer: acrLoginServer
    containerImageName: containerImageName
    acrPullIdentityId: acrPullIdentityId
    spotifyClientId: spotifyClientId
    spotifyClientSecret: spotifyClientSecret
    spotifyRedirectUri: spotifyRedirectUri
    setlistFmClientSecret: setlistFmClientSecret
  }
}

output APP_URL string = resources.outputs.appUrl
