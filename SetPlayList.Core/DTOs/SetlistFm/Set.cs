namespace SetPlayList.Core.DTOs.SetlistFm;

public record Set(
    string? Name,
    int Encore,
    List<Song> Song);
