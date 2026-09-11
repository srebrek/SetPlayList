# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SetPlayList is a portfolio web app (ASP.NET Core 8 + Blazor Server, Bootstrap UI) that lets a user paste a setlist.fm concert link and generates a Spotify playlist from the songs in that setlist, with a preview/editing step before saving to the user's Spotify account.

Note: Spotify now restricts the `playlist-modify-*` scopes to apps with 250k+ MAU, so the live/deployed version cannot actually create playlists on behalf of arbitrary users (see the `notRegistered`/Forbidden path in `PlaylistPreview.razor`).

## Commands

```bash
dotnet build                                    # build the whole solution
dotnet run --project SetPlayList.Api            # run the app (https://localhost:7101, http://localhost:5004)
dotnet test                                      # run all tests
dotnet test --filter "FullyQualifiedName~SpotifyAuthServiceTests"   # run a single test class
dotnet test --filter "DisplayName~HandleAuthorizationCallbackAsync" # run tests matching a name
```

Before running locally, set Spotify/Setlist.fm credentials in `SetPlayList.Api/appsettings.json` (or via environment variables / user-secrets) under the `Spotify` (`ClientId`, `ClientSecret`, `RedirectUri`) and `SetlistFm` (`ClientSecret`) sections. `RedirectUri` defaults to `https://localhost:7101/auth/spotify/callback`.

## Architecture

Three projects:
- **SetPlayList.Core** — DTOs (`DTOs/SetlistFm`, `DTOs/Spotify` — shapes matching the external APIs' JSON), domain `Models` (`ProposedPlaylist`, `ProposedTrack`, `SpotifyTrack`, `Song` — the app's own preview/edit model), and `Interfaces` for all clients/services. No implementation logic lives here.
- **SetPlayList.Api** — the ASP.NET Core Blazor Server app. `Clients/` talk to the external HTTP APIs, `Services/` hold business logic, `Components/Pages` are the Blazor routable pages, `Configuration/` are `IOptions`-bound settings classes, `Support/` has small Blazor helpers (e.g. `BootstrapFieldCssClassProvider` for Bootstrap validation CSS classes).
- **SetPlayList.Api.Tests** — xUnit + Moq + `RichardSzalay.MockHttp`, mirrors the `Clients`/`Services` structure under `UnitTests/`.

### Request flow (the core feature)

1. `SetPlayListPage.razor` (`/setplaylist`) — user pastes a setlist.fm URL; a regex extracts the setlist ID from the URL and navigates to `/setplaylist/preview/{SetlistId}`.
2. `PlaylistPreview.razor` (`/setplaylist/preview/{SetlistId}`) — on init, checks for a Spotify access-token cookie (via `ISpotifyAuthService`); if missing, shows a login link (`/auth/spotify/login`). If present, calls `ISpotifyPlaylistService.GeneratePreviewAsync` to fetch the setlist from setlist.fm and, for every song, search Spotify (in parallel) for track candidates. The user picks one Spotify track per song via radio buttons, then submits to `ISpotifyPlaylistService.CreatePlaylistOnSpotifyAsync`, which creates the playlist and adds the chosen tracks.
3. `Program.cs` also exposes plain minimal-API endpoints (`/auth/spotify/login`, `/auth/spotify/callback`, `api/setlist/{setlistId}`, `api/playlist-preview/{setlistId}`) alongside the Razor components — some overlap with page logic exists (e.g. the preview endpoint duplicates what `PlaylistPreview.razor` does inline).

### Auth model

`SpotifyAuthService` implements the OAuth code flow using two cookies: a short-lived `spotify_auth_state` (CSRF state, 5 min) and `spotify_token` (access token, 30 min). There is currently no refresh-token handling (see the `// TODO: Add token refresh` in `SpotifyAuthService`) — the token cookie simply expires.

### API client conventions

`SpotifyApiClient` and `SetlistFmApiClient` follow a consistent pattern worth matching when extending them:
- Every call returns a tuple `(T? result, HttpStatusCode httpStatusCode)` rather than throwing on API failures — callers branch on the status code.
- JSON is deserialized with `JsonNamingPolicy.SnakeCaseLower` (Spotify) or `JsonSerializerDefaults.Web` (setlist.fm), matching each external API's casing.
- Errors are caught (`HttpRequestException`, `JsonException`, generic `Exception`) and logged via `ILogger`, returning `BadGateway`/`InternalServerError` rather than propagating exceptions, so unhandled-path code in services still uses `throw new NotImplementedException()` as a placeholder for cases not yet designed (this is an existing pattern in the codebase, not a convention to introduce elsewhere).

### Domain models vs DTOs

DTOs in `SetPlayList.Core/DTOs` mirror the external APIs' JSON shape exactly. `SetPlayList.Core/Models/TempModels.cs` holds the app's own view/edit model (`ProposedPlaylist` → `ProposedTrack` → `SpotifyOptions`/`SelectedTrack`) used to drive the preview/edit UI and round-trip user selections through Blazor's model binder — note the parameterless constructors on these models exist specifically for that binder.
