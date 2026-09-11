namespace SetPlayList.Integrations.SetlistFm.Dtos;

internal sealed record Song(
    string Name,
    Artist? With,
    Artist? Cover,
    string? Info,
    bool Tape);
