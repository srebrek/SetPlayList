using System.Text.Json.Serialization;

namespace SetPlayList.Integrations.SetlistFm;

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

internal sealed record Artist(string MBId, string Name, string SortName, string Disambiguation, string Url);

internal sealed record Venue(City City, string Url, string Id, string Name);

internal sealed record City(string Id, string Name, string StateCode, string State, Coords Coords, Country Country);

internal sealed record Coords(
    [property: JsonPropertyName("long")] double Longitude,
    [property: JsonPropertyName("lat")] double Latitude);

internal sealed record Country(string Code, string Name);

internal sealed record Tour(string Name);

internal sealed record Sets(List<Set> Set);

internal sealed record Set(string? Name, int Encore, List<Song> Song);

internal sealed record Song(string Name, Artist? With, Artist? Cover, string? Info, bool Tape);
