namespace SetPlayList.Core.Models;

public class ProposedPlaylist(string name, List<ProposedTrack> tracks)
{
    public string Name { get; set; } = name;
    public List<ProposedTrack> Tracks { get; set; } = tracks;
}

public class ProposedTrack(Song song, List<SpotifyTrack> spotifyOptions, SpotifyTrack selectedTrack)
{
    public Song OriginalSong { get; set; } = song;
    public List<SpotifyTrack> SpotifyOptions { get; set; } = spotifyOptions;
    public SpotifyTrack? SelectedTrack { get; set; } = selectedTrack;
    public string? SelectedTrackId 
    {
        get => SelectedTrack?.Id;
        set
        {
            SelectedTrack = SpotifyOptions.Where(spotifyOption => spotifyOption.Id == value).FirstOrDefault();
        }
    }
}

public class SpotifyTrack(string id, string name, List<string> artists, string albumName, string albumImageUrl)
{
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public List<string> Artists { get; set; } = artists;
    public string AlbumName { get; set; } = albumName;
    public string AlbumImageUrl { get; set; } = albumImageUrl;
}

public class Song(string name, string? artist, string? originalArtist, bool isTape)
{
    public string Name { get; set; } = name;
    public string? With { get; set; } = artist;
    public string? OriginalArtist { get; set; } = originalArtist;
    public bool IsTape { get; set; } = isTape;
}