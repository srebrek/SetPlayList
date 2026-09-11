# Deployment

Azure Container Apps. The pipeline is `.github/workflows/cicd.yml`; there is no IaC in the repo - the
workflow only builds the image and repoints the running app. This file records what was set up by hand.

Live at <https://setplaylist.zlotekmikolaj.com>.

## `rg-shared`, Poland Central

Shared with PokerManager, nothing here was created for this app.

| Resource | What | Used for |
| --- | --- | --- |
| `acrsrebrek` | container registry | holds the `setplaylist` repository |
| `ace-shared` | Container Apps environment | hosts the app and its managed certificate |
| `id-github-ci` | user-assigned identity | deploy identity; added Federated Credential `github-setplaylist-master` = `repo:srebrek/SetPlayList:ref:refs/heads/master` |

## `rg-setplaylist`, Poland Central

| Resource | What | Chosen by hand |
| --- | --- | --- |
| `id-setplaylist` | user-assigned identity | the app's runtime identity, `AcrPull` on `acrsrebrek` and nothing else |
| `setplaylist` | container app | in `ace-shared`, external ingress on port 8080, 0-1 replicas, 0.5 vCPU / 1 Gi, image pulled with `id-setplaylist` |

The runtime identity is deliberately not `id-github-ci`: that one is Contributor + RBAC Administrator at
subscription scope, and a container holding it would hand the whole subscription to anyone who got code
execution in the app.

Scale-to-zero is on. Data-protection keys live in the container, so a cold start drops login cookies and
breaks a login that was mid-flight; fine for a demo, `--min-replicas 1` is the fix if it ever isn't.

### Secrets

Container App secrets `spotify-client-id`, `spotify-client-secret`, `setlistfm-client-secret`, surfaced
as `Spotify__ClientId`, `Spotify__ClientSecret`, `SetlistFm__ClientSecret`. `Spotify__RedirectUri` is a
plain value, `https://setplaylist.zlotekmikolaj.com/signin-spotify`, and has to match the redirect URI
registered in the Spotify developer dashboard.

```bash
az containerapp secret set --name setplaylist --resource-group rg-setplaylist \
  --secrets spotify-client-secret=<new-value>
az containerapp revision restart --name setplaylist --resource-group rg-setplaylist \
  --revision "$(az containerapp show -n setplaylist -g rg-setplaylist \
    --query properties.latestRevisionName -o tsv)"
```

## DNS and certificate

`zlotekmikolaj.com` is on Cloudflare. Two records, both added by hand:

| Type | Name | Content | Proxy |
| --- | --- | --- | --- |
| CNAME | `setplaylist` | `setplaylist.orangefield-371afb5b.polandcentral.azurecontainerapps.io` | DNS only |
| TXT | `asuid.setplaylist` | the app's `customDomainVerificationId` | - |

The TXT record is what `az containerapp hostname add` checks; without it the call fails with
`InvalidCustomHostNameValidation`. Binding then issues a free managed certificate (up to ~20 minutes),
which is stored on the environment, not the app:

```bash
az containerapp hostname add -n setplaylist -g rg-setplaylist \
  --hostname setplaylist.zlotekmikolaj.com

az containerapp hostname bind -n setplaylist -g rg-setplaylist \
  --hostname setplaylist.zlotekmikolaj.com \
  --environment /subscriptions/<sub>/resourceGroups/rg-shared/providers/Microsoft.App/managedEnvironments/ace-shared \
  --validation-method CNAME
```

`--environment` needs the full resource id, because the environment lives in a different resource group
than the app.

## GitHub

Secrets `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` (the `id-github-ci` client id, the
tenant and the subscription - no passwords, the login is federated).

The workflow pushes to ACR without Docker: `az acr login --expose-token` gives a token that the SDK
container publish picks up through `SDK_CONTAINER_REGISTRY_UNAME` / `SDK_CONTAINER_REGISTRY_PWORD`.

## Spotify developer dashboard

Redirect URIs must list `https://setplaylist.zlotekmikolaj.com/signin-spotify` alongside the local
`https://localhost:7101/signin-spotify`. The app is in Development Mode, so every account that is going
to create a playlist has to be added under User Management (25 max); everyone else gets a 403.
