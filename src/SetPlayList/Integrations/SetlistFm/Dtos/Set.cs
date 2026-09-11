namespace SetPlayList.Integrations.SetlistFm.Dtos;

internal sealed record Set(string? Name, int Encore, List<Song> Song);
