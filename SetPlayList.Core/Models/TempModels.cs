namespace SetPlayList.Core.Models;

public class ProposedPlaylist
{
    public string Name { get; set; }
    public List<ProposedTrack> Tracks { get; set; }

    // Konstruktor bezparametrowy dla Model Bindera
    public ProposedPlaylist()
    {
        Name = string.Empty;
        Tracks = new List<ProposedTrack>();
    }

    // Twój istniejący konstruktor
    public ProposedPlaylist(string name, List<ProposedTrack> tracks)
    {
        Name = name;
        Tracks = tracks;
    }
}

public class ProposedTrack
{
    public Song OriginalSong { get; set; }
    public List<SpotifyTrack> SpotifyOptions { get; set; }
    public SpotifyTrack? SelectedTrack { get; private set; }
    public string? SelectedTrackId
    {
        get => SelectedTrack?.Id;
        set
        {
            SelectedTrack = SpotifyOptions?.FirstOrDefault(spotifyOption => spotifyOption.Id == value);
        }
    }

    // Konstruktor bezparametrowy dla Model Bindera
    public ProposedTrack()
    {
        OriginalSong = new Song(); // Inicjalizujemy, aby uniknąć NullReference
        SpotifyOptions = new List<SpotifyTrack>();
    }

    // Twój istniejący konstruktor
    public ProposedTrack(Song song, List<SpotifyTrack> spotifyOptions, SpotifyTrack selectedTrack)
    {
        OriginalSong = song;
        SpotifyOptions = spotifyOptions;
        SelectedTrack = selectedTrack;
    }
}

public class SpotifyTrack
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> Artists { get; set; }
    public string AlbumName { get; set; }
    public string AlbumImageUrl { get; set; }

    // Konstruktor bezparametrowy dla Model Bindera
    public SpotifyTrack()
    {
        Id = string.Empty;
        Name = string.Empty;
        Artists = new List<string>();
        AlbumName = string.Empty;
        AlbumImageUrl = string.Empty;
    }

    // Twój istniejący konstruktor
    public SpotifyTrack(string id, string name, List<string> artists, string albumName, string albumImageUrl)
    {
        Id = id;
        Name = name;
        Artists = artists;
        AlbumName = albumName;
        AlbumImageUrl = albumImageUrl;
    }
}

public class Song
{
    public string Name { get; set; }
    public string? With { get; set; }
    public string? OriginalArtist { get; set; }
    public bool IsTape { get; set; }

    // Konstruktor bezparametrowy dla Model Bindera
    public Song()
    {
        Name = string.Empty;
    }

    // Twój istniejący konstruktor
    public Song(string name, string? artist, string? originalArtist, bool isTape)
    {
        Name = name;
        With = artist;
        OriginalArtist = originalArtist;
        IsTape = isTape;
    }
}