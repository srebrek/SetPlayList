namespace SetPlayList.Integrations.SetlistFm.Dtos;

internal sealed record City(
    string Id,
    string Name,
    string StateCode,
    string State,
    Coords Coords,
    Country Country);
