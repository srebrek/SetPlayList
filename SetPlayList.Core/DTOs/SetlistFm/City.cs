namespace SetPlayList.Core.DTOs.SetlistFm;

public record City(
    string Id,
    string Name,
    string StateCode,
    string State,
    Coords Coords,
    Country Country);
