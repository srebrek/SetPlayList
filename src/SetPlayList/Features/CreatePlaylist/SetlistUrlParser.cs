using System.Text.RegularExpressions;
using SetPlayList.Common;

namespace SetPlayList.Features.CreatePlaylist;

internal static partial class SetlistUrlParser
{
    public static Result<string> ParseId(string setlistUrl)
    {
        Match match = IdPattern().Match(setlistUrl);
        return match.Success
            ? match.Groups[1].Value
            : Error.Validation("Could not find a setlist id in that URL. Paste a link like https://www.setlist.fm/setlist/artist/2022/venue-33b2b0fd.html.");
    }

    [GeneratedRegex(@"-([a-f0-9]+)\.html$", RegexOptions.IgnoreCase)]
    private static partial Regex IdPattern();
}
