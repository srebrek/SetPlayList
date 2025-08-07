namespace SetPlayList.Core.DTOs.SetlistFm;

public record Song(
    string Name,
    Artist? With,
    Artist? Cover,
    string? Info,
    bool Tape);