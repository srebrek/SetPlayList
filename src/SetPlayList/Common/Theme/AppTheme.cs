using MudBlazor;

namespace SetPlayList.Common.Theme;

/// <summary>
/// Central place for the application's MudBlazor theme. Adjust palette here.
/// </summary>
internal static class AppTheme
{
    public static MudTheme Default { get; } = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#ffc107",
            Secondary = "#3f51b5",
            Surface = "#1e1e2d",
            Background = "#161b19",
            BackgroundGray = "#151521",
            AppbarText = "#92929f",
            AppbarBackground = "#161b19",
            DrawerBackground = "#161b19",
            ActionDefault = "#74718e",
            ActionDisabled = "#9999994d",
            ActionDisabledBackground = "#605f6d4d",
            TextPrimary = "#b2b0bf",
            TextSecondary = "#92929f",
            TextDisabled = "#ffffff33",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#ffb545",
            Error = "#ff3f5f",
            LinesDefault = "#33323e",
            TableLines = "#33323e",
            Divider = "#292838",
            OverlayLight = "#1e1e2d80",
        },
        LayoutProperties = new LayoutProperties(),
    };
}
