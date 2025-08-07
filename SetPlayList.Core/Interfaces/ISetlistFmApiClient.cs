using SetPlayList.Core.DTOs.SetlistFm;

namespace SetPlayList.Core.Interfaces;

public interface ISetlistFmApiClient
{
    Task<Setlist?> GetSetlistAsync(string setlistId);
}
