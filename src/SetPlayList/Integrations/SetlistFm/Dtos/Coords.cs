using System.Text.Json.Serialization;

namespace SetPlayList.Integrations.SetlistFm.Dtos;

internal sealed record Coords(
    [property: JsonPropertyName("long")] double Longitude,
    double Lat);
