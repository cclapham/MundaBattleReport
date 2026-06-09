using MudBlazor;

namespace MundaBattleReport.Themes;

public static class MundaThemeFactory
{
    public static MudTheme GetDarkTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Black = "#0a0e27",
                White = "#f5f5f5",
                Primary = "#d4af37", // Gold from logo
                PrimaryContrastText = "#0a0e27",
                Secondary = "#4da6ff", // Blue energy core
                SecondaryContrastText = "#ffffff",
                Tertiary = "#8b4545", // Deep red/purple
                TertiaryContrastText = "#ffffff",
                Info = "#4da6ff", // Blue - matches secondary
                Success = "#4caf50", // Bright green for success feedback
                Warning = "#ffb74d", // Orange for caution
                Error = "#ef5350", // Red for errors
                Dark = "#1a1a1a",
                TextPrimary = "#e8e8e8",
                TextSecondary = "#b0b0b0",
                TextDisabled = "#666666",
                Background = "#0a0e27",
                Surface = "#2d2d2d",
                DrawerBackground = "#1a1a1a",
                DrawerText = "#e8e8e8",
                AppbarBackground = "#0a0e27",
                AppbarText = "#d4af37",
                LinesDefault = "#404040",
                TableLines = "#404040",
                DividerLight = "#333333",
                HoverOpacity = 0.15f,
            }
        };
    }

    public static MudTheme GetLightTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Black = "#000000",
                White = "#ffffff",
                Primary = "#b8941d", // Darker gold for light theme
                PrimaryContrastText = "#ffffff",
                Secondary = "#1a9fff", // Brighter blue
                SecondaryContrastText = "#ffffff",
                Tertiary = "#8b4545",
                TertiaryContrastText = "#ffffff",
                Info = "#0080ff",
                Success = "#00aa44",
                Warning = "#ff9500",
                Error = "#d32f2f",
                Dark = "#424242",
                TextPrimary = "#212121",
                TextSecondary = "#757575",
                TextDisabled = "#bdbdbd",
                Background = "#fafafa",
                Surface = "#ffffff",
                DrawerBackground = "#ffffff",
                DrawerText = "#212121",
                AppbarBackground = "#f5f5f5",
                AppbarText = "#212121",
                LinesDefault = "#e0e0e0",
                TableLines = "#e0e0e0",
                DividerLight = "#f5f5f5",
                HoverOpacity = 0.08f,
            }
        };
    }
}
