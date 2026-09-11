namespace SetPlayList.Integrations.SetlistFm.Dtos;

internal sealed record Setlist(
    Artist Artist,
    Venue Venue,
    Tour Tour,
    Sets Sets,
    string Info,
    string Url,
    string Id,
    string VersionId,
    string EventDate,
    string LastUpdated);
