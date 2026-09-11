namespace SetPlayList.Integrations.SetlistFm.Dtos;

internal sealed record Artist(
    string MBId,
    string Name,
    string SortName,
    string Disambiguation,
    string Url);
