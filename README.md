# SetPlayList

Fetches a saved concert setlist from [setlist.fm](https://www.setlist.fm/) and turns it into a Spotify
playlist, with a preview step where each song's Spotify match can be swapped before saving.
Live at <https://setplaylist.zlotekmikolaj.com>.

Blazor Server (net10.0) + MudBlazor, sliced by feature. The portfolio content that used to live in this
repo moved to a separate static site.

## Important note about the Spotify API

Spotify now requires apps to be approved for Extended Quota Mode before they can create playlists on
behalf of arbitrary users; unapproved apps are capped at 25 allow-listed accounts (Development Mode).
Playlist creation will fail with a 403 for anyone not on that allow-list — see the showcase video at
`https://zlotekmikolaj.com#setPlayList` for a full walkthrough instead.

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

## Deployment

Live at <https://setplaylist.zlotekmikolaj.com>, on Azure Container Apps. Pushing to `master` runs
`.github/workflows/cicd.yml`, which builds the image with the SDK container publish, pushes it to the
shared ACR and repoints the Container App at the new tag.

There is no IaC here — the infrastructure was created once with `az` and is written down in
[`docs/deployment.md`](docs/deployment.md), together with the DNS, certificate and Spotify dashboard
setup.
