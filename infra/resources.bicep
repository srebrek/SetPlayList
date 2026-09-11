param environmentName string
param location string
param acrLoginServer string
param containerImageName string
param acrPullIdentityId string

@secure()
param spotifyClientId string

@secure()
param spotifyClientSecret string

param spotifyRedirectUri string

@secure()
param setlistFmClientSecret string

var appName = 'setplaylist-${environmentName}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-${appName}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: 'cae-${appName}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: appName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${acrPullIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
      }
      registries: [
        {
          server: acrLoginServer
          identity: acrPullIdentityId
        }
      ]
      secrets: [
        {
          name: 'spotify-client-id'
          value: spotifyClientId
        }
        {
          name: 'spotify-client-secret'
          value: spotifyClientSecret
        }
        {
          name: 'setlistfm-client-secret'
          value: setlistFmClientSecret
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'app'
          image: containerImageName
          env: [
            { name: 'Spotify__ClientId', secretRef: 'spotify-client-id' }
            { name: 'Spotify__ClientSecret', secretRef: 'spotify-client-secret' }
            { name: 'Spotify__RedirectUri', value: spotifyRedirectUri }
            { name: 'SetlistFm__ClientSecret', secretRef: 'setlistfm-client-secret' }
          ]
          probes: [
            {
              type: 'Readiness'
              httpGet: {
                path: '/health'
                port: 8080
              }
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

output appUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
