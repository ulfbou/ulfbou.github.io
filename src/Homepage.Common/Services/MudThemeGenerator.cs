using MudBlazor;
using MudBlazor.Utilities;

namespace Homepage.Common.Services;

public class MudThemeGenerator
{
    private static Random random = new Random();
    public MudTheme CurrentTheme { get; set; }

    public MudTheme GenerateMonochromeTheme()
    {
        // Generate random hue (0-360) and saturation (30-70%)
        double hue = random.Next(0, 361);
        double saturation = random.NextDouble() * (70 - 30) + 30;

        var paletteLight = GeneratePalette<PaletteLight>(hue, saturation);

        // Define other theme properties based on primary color
        var theme = new MudTheme
        {
            PaletteLight = paletteLight,
            PaletteDark = GeneratePalette<PaletteDark>(hue, saturation),
        };
        return theme;
    }

    public TPalette GeneratePalette<TPalette>(double hue, double saturation) where TPalette : Palette, new()
    {
        bool isDark = typeof(TPalette) == typeof(PaletteDark);
        // Convert primary color from HSL to RGB
        var primaryColor = HSLToRGB(hue, saturation, 50);

        double l0 = isDark ? 0 : 1;
        double l1 = isDark ? 0.1 : 0.9;
        double l2 = isDark ? 0.2 : 0.8;
        double l3 = isDark ? 0.3 : 0.7;
        double l4 = isDark ? 0.4 : 0.6;
        double l5 = isDark ? 0.5 : 0.5;
        double l6 = isDark ? 0.6 : 0.4;
        double l7 = isDark ? 0.7 : 0.3;
        double l8 = isDark ? 0.8 : 0.2;
        double l9 = isDark ? 0.9 : 0.1;
        double l95 = isDark ? 0.95 : 0.05;
        double l10 = isDark ? 1 : 0;

        // Define other palette properties based on primary color
        var palette = new TPalette
        {
            Primary = primaryColor,
            PrimaryContrastText = Color(hue, saturation, l10),
            TextPrimary = Color(hue, saturation, l2),
            Secondary = Color(hue, saturation, l4),
            SecondaryContrastText = Color(hue, saturation, l9),
            TextSecondary = Color(hue, saturation, l3),
            Tertiary = Color(hue, saturation, l6),
            TertiaryContrastText = Color(hue, saturation, l95),
            Background = Color(hue, saturation, l95),
            BackgroundGray = Color(hue, 0.1, l9),
            Info = Color(hue, saturation, l5),
            InfoContrastText = Color(hue, saturation, l10),
            Success = Color(hue, saturation, l5),
            SuccessContrastText = Color(hue, saturation, l10),
            Warning = Color(hue, saturation, l5),
            WarningContrastText = Color(hue, saturation, l10),
            Error = Color(hue, saturation, l5),
            ErrorContrastText = Color(hue, saturation, l10),
            Dark = Color(hue, saturation, l1),
            DarkContrastText = Color(hue, saturation, l9),
            TextDisabled = Color(hue, 0.15, l7),
        };
        return palette;
    }

    private static MudColor Color(double hue, double saturation, double lightness)
    {
        return new MudColor(hue, saturation, lightness / 100, 255);
    }

    private static string HSLToRGB(double hue, double saturation, double lightness)
    {
        // Conversion logic from HSL to RGB
        double c = (1 - Math.Abs(2 * lightness / 100 - 1)) * (saturation / 100);
        double x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
        double m = lightness / 100 - c / 2;
        double r = 0, g = 0, b = 0;
        if (hue < 60)
        {
            r = c; g = x;
        }
        else if (hue < 120)
        {
            r = x; g = c;
        }
        else if (hue < 180)
        {
            g = c;
            b = x;
        }
        else if (hue < 240)
        {
            g = x;
            b = c;
        }
        else if (hue < 300)
        {
            r = x;
            b = c;
        }
        else
        {
            r = c;
            b = x;
        }

        r = Math.Round((r + m) * 255);
        g = Math.Round((g + m) * 255);
        b = Math.Round((b + m) * 255);
        return $"#{(int)r:X2}{(int)g:X2}{(int)b:X2}";
    }
}