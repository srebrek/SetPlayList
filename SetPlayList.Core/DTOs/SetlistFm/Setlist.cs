namespace SetPlayList.Core.DTOs.SetlistFm;

public record Setlist(
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
