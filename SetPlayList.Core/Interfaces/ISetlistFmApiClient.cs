using SetPlayList.Core.DTOs.SetlistFm;
using System.Net;

namespace SetPlayList.Core.Interfaces;

public interface ISetlistFmApiClient
{
    Task<(Setlist? setlist, HttpStatusCode httpStatusCode)> GetSetlistAsync(string setlistId);
}
