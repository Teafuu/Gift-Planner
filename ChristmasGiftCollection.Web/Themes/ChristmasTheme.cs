using MudBlazor;

namespace ChristmasGiftCollection.Web.Themes;

/// <summary>
/// Christmas-themed dark mode palette for the Gift Collection application.
/// Primary: Christmas red, Secondary: Christmas green, Tertiary: Gold
/// </summary>
public static class ChristmasTheme
{
    public static MudTheme Theme { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#c62828",        // Christmas red
            Secondary = "#2e7d32",      // Christmas green
            Tertiary = "#ffd700",       // Gold
            AppbarBackground = "#1b5e20",
            Background = "#ffffff",
            Surface = "#ffffff",
            DrawerBackground = "#ffffff",
            DrawerText = "rgba(0,0,0, 0.7)",
            AppbarText = "rgba(255,255,255, 0.9)",
            TextPrimary = "rgba(0,0,0, 0.87)",
            TextSecondary = "rgba(0,0,0, 0.54)",
            ActionDefault = "#474747",
            ActionDisabled = "rgba(0,0,0, 0.26)",
            ActionDisabledBackground = "rgba(0,0,0, 0.12)",
            Divider = "rgba(0,0,0, 0.12)",
            DividerLight = "rgba(0,0,0, 0.06)",
            TableLines = "rgba(0,0,0, 0.12)",
            LinesDefault = "rgba(0,0,0, 0.12)",
            LinesInputs = "rgba(0,0,0, 0.42)",
            TextDisabled = "rgba(0,0,0, 0.38)"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#ef5350",        // Lighter Christmas red for dark mode
            Secondary = "#66bb6a",      // Lighter Christmas green
            Tertiary = "#ffd700",       // Gold
            AppbarBackground = "#1b5e20",
            Background = "#121212",
            Surface = "#1e1e1e",
            DrawerBackground = "#1e1e1e",
            DrawerText = "rgba(255,255,255, 0.7)",
            AppbarText = "rgba(255,255,255, 0.9)",
            TextPrimary = "rgba(255,255,255, 0.9)",
            TextSecondary = "rgba(255,255,255, 0.7)",
            ActionDefault = "#adadb1",
            ActionDisabled = "rgba(255,255,255, 0.3)",
            ActionDisabledBackground = "rgba(255,255,255, 0.12)",
            Divider = "rgba(255,255,255, 0.12)",
            DividerLight = "rgba(255,255,255, 0.06)",
            TableLines = "rgba(255,255,255, 0.12)",
            LinesDefault = "rgba(255,255,255, 0.12)",
            LinesInputs = "rgba(255,255,255, 0.3)",
            TextDisabled = "rgba(255,255,255, 0.2)",
            Info = "#3299ff",
            Success = "#0bba83",
            Warning = "#ffa800",
            Error = "#f44336",
            Dark = "#27272f"
        }
    };
}
