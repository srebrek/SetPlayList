# SetPlayList

Fetches a saved concert setlist from [setlist.fm](https://www.setlist.fm/) and turns it into a Spotify
playlist, with a preview step where each song's Spotify match can be swapped before saving.

Blazor Server (net10.0) + MudBlazor, sliced by feature. The portfolio content that used to live in this
repo moved to a separate static site.

## Important note about the Spotify API

Spotify now requires apps to be approved for Extended Quota Mode before they can create playlists on
behalf of arbitrary users; unapproved apps are capped at 25 allow-listed accounts (Development Mode).
Playlist creation will fail with a 403 for anyone not on that allow-list — see the showcase video at
`https://TODO-DOMAIN#setPlayList` for a full walkthrough instead.

## Running locally

```bash
dotnet user-secrets set "Spotify:ClientId" "..." --project src/SetPlayList
dotnet user-secrets set "Spotify:ClientSecret" "..." --project src/SetPlayList
dotnet user-secrets set "Spotify:RedirectUri" "https://localhost:7101/signin-spotify" --project src/SetPlayList
dotnet user-secrets set "SetlistFm:ClientSecret" "..." --project src/SetPlayList

dotnet run --project src/SetPlayList
```

The app is reachable at `https://localhost:7101`. Without any secrets configured it still renders (home
page, the "not logged in" preview state, `/health`) — only the Spotify login/playlist-creation path
needs real credentials.

Data-protection keys are kept in-container, so a scale-to-zero cold start invalidates existing login
cookies (users just log in again) — acceptable for a demo; persisting keys would need blob storage + Key
Vault.

## Container

No Dockerfile — SDK container publish, same as the reference app this was styled after:

```bash
dotnet publish src/SetPlayList/SetPlayList.csproj \
  -p:PublishProfile=DefaultContainer \
  -p:ContainerRegistry=<acr-login-server>
```

## Deployment (TODO before it actually works)

- [ ] Create/point at the shared ACR and fill in `ACR_LOGIN_SERVER`, `CONTAINER_APP_NAME`,
      `RESOURCE_GROUP` in `.github/workflows/deploy.yml`.
- [ ] Fill in `acrLoginServer`, `containerImageName`, `acrPullIdentityId` in `infra/main.parameters.json`.
- [ ] Set the `AZURE_CLIENT_ID` / `AZURE_TENANT_ID` / `AZURE_SUBSCRIPTION_ID` GitHub secrets for the
      federated-credential login used by the deploy workflow.
- [ ] Set `SPOTIFY_CLIENT_ID` / `SPOTIFY_CLIENT_SECRET` / `SETLISTFM_CLIENT_SECRET` for the
      `az deployment` / `azd provision` step (see `infra/main.parameters.json`).
- [ ] Pick the real subdomain and update `spotifyRedirectUri` in `infra/main.parameters.json` (must also
      match the redirect URI registered in the Spotify developer dashboard).
- [ ] `azd` is not installed locally (only `az`) — use `az deployment sub what-if` against
      `infra/main.bicep` before any real deploy, or install `azd` first.

Custom subdomain binding is a manual/CLI step (Container Apps custom domain + certificate), not part of
these files.
