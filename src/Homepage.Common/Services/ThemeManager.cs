using Homepage.Common.Extensions;
using Homepage.Common.Utils;

using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Utilities;

using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel;

namespace Homepage.Common.Services;

public class ThemeManager
{
    private MudTheme _currentTheme = new MudTheme();
    private readonly Random _random = new Random();

    public MudTheme GenerateTheme(double? backgroundHue = null)
    {
        backgroundHue ??= _random.NextDouble() * 360;
        double backgroundSaturation = _random.NextDouble() * 0.5 + 0.25;
        double backgroundLightness = _random.NextDouble() * 0.2 + 0.6;
        double backgroundAlpha = _random.NextDouble() * 0.5 + 0.5;

        var monoChronicColors = GenerateMonoChronicColors(backgroundHue.Value, backgroundSaturation, backgroundAlpha);

        PaletteLight paletteLight = GeneratePalette<PaletteLight>(backgroundHue.Value, backgroundSaturation, backgroundLightness, backgroundAlpha);
        PaletteDark paletteDark = GeneratePalette<PaletteDark>(backgroundHue.Value, backgroundSaturation, backgroundLightness, backgroundAlpha);

        _currentTheme = new MudTheme()
        {
            PaletteLight = paletteLight,
            PaletteDark = paletteDark
        };

        OnThemeChanged.InvokeAsync(_currentTheme);

        return _currentTheme;
    }

    private IEnumerable<MudColor> GenerateMonoChronicColors(double startHue, double startSaturation, double alpha)
    {
        double endHue = startHue + _random.NextDouble() * 60 - 30;
        double endSaturation = startSaturation + _random.NextDouble() * 0.2 - 0.1;
        double diffHue = (endHue - startHue) / 10;
        double diffSaturation = (endSaturation - startSaturation) / 10;
        double currentHue = startHue;
        double currentSaturation = startSaturation;

        for (int i = 1; i < 10; i++)
        {
            if (i < 4 || i > 6)
            {
                double hue = currentHue + _random.NextDouble() * diffHue - diffHue / 2;
                double saturation = currentSaturation + _random.NextDouble() * 0.1 - 0.05;
                double lightness = 0.1 * i + _random.NextDouble() * 0.1 - 0.05;

                yield return new MudColor(startHue, saturation, lightness, alpha);
            }
            currentHue += diffHue;
            currentSaturation += diffSaturation;
        }
    }
    private TPalette GeneratePalette<TPalette>(IEnumerable<MudColor> colors) where TPalette : Palette, new()
    {
        bool isLight = typeof(TPalette) == typeof(PaletteLight);
        if (colors.Count() != 6)
        {
            throw new ArgumentException("Color list contain {colors.Count()}, but expected 6 colors. ");
        }

        if (!isLight)
        {
            colors = colors.Reverse();
        }

        MudColor[] mudColors = colors.ToArray();
        MudColor dark1 = mudColors[0];
        MudColor dark2 = mudColors[1];
        MudColor dark3 = mudColors[2];
        MudColor light3 = mudColors[3];
        MudColor light2 = mudColors[4];
        MudColor light1 = mudColors[5];

        MudColor[] grayColors = colors.Select(c => new MudColor(c.H, _random.NextDouble() / 10, c.L, c.A)).ToArray();
        MudColor black = grayColors[0];
        MudColor gray1 = grayColors[1];
        MudColor gray2 = grayColors[2];
        MudColor gray3 = grayColors[3];
        MudColor gray4 = grayColors[4];
        MudColor white = grayColors[5];

        // Generate an info color that has contrast with the background



        return new TPalette
        {
            White = white,
            Black = black,
            Primary = dark2,
            PrimaryContrastText = light2,
            TextPrimary = dark2,
            Secondary = light3,
            SecondaryContrastText = dark1,
            TextSecondary = dark2,
            Tertiary = light1,
            TertiaryContrastText = dark2,
            Background = light2,
            BackgroundGray = gray2,
            //Info = ,
            //InfoContrastText = infoContrastText,
            //Success = success,
            //SuccessContrastText = successContrastText,
            //Warning = warning,
            //WarningContrastText = warningContrastText,
            //Error = error,
            //ErrorContrastText = errorContrastText,
            //Dark = dark,
            //DarkContrastText = darkContrastText,
            //TextPrimary = primaryContrastText,
            //TextSecondary = secondaryContrastText,
            //TextDisabled = disabled,
            //Background = backgroundColor,
            //BackgroundGray = isLight ? new MudColor(backgroundColor.H, backgroundColor.S, 0.95, 255) : new MudColor(backgroundColor.H, backgroundColor.S, 0.05, 255),
            //AppbarBackground = primaryContrastText,
            //AppbarText = primary,
            //DrawerBackground = secondaryContrastText,
            //DrawerText = secondary

            Info = light1,
            InfoContrastText = dark3

        };
    }

    private TPalette GeneratePalette<TPalette>(double hue, double saturation, double lightness, double alpha) where TPalette : Palette, new()
    {
        bool isLight = typeof(TPalette) == typeof(PaletteLight);
        MudColor backgroundColor = new MudColor(hue, saturation, lightness, alpha);

        double light = isLight ? 0.9 : 0.1;
        MudColor white = new MudColor(ModifyHue(hue), ModifySaturation(saturation), light, 255);
        MudColor black = new MudColor(ModifyHue(hue), ModifySaturation(saturation), 1 - light, 255);

        double primaryHue = ModifyHue(hue);
        double primarySaturation = ModifySaturation(saturation);
        double primaryLightness = FlipLightness(lightness);
        MudColor primary = new MudColor(primaryHue, primarySaturation, primaryLightness, 255);
        MudColor primaryContrastText = primary.ToContrastingColor();
        // Ensure minimum contrast for text and background
        double minContrastRatio = 4.5; // WCAG 2.1 AA standard

        // Adjust colors to meet contrast requirements
        (primary, backgroundColor) = EnsureContrast(primary, backgroundColor, minContrastRatio);
        (primary, primaryContrastText) = EnsureContrast(primary, primaryContrastText, minContrastRatio);

        double secondaryHue = ModifyHue(primaryHue, 0.2);
        double secondarySaturation = ModifySaturation(primarySaturation);
        double secondaryLightness = ModifyLightness(primaryLightness);
        MudColor secondary = new MudColor(secondaryHue, secondarySaturation, secondaryLightness, 255);
        MudColor secondaryContrastText = secondary.ToContrastingColor();

        (secondary, secondaryContrastText) = EnsureContrast(secondary, secondaryContrastText, minContrastRatio);

        double tertiaryHue = ModifyHue(secondaryHue, 0.2);
        double tertiarySaturation = ModifySaturation(secondarySaturation);
        double tertiaryLightness = ModifyLightness(secondaryLightness);
        MudColor tertiary = new MudColor(tertiaryHue, tertiarySaturation, tertiaryLightness, 255);
        MudColor tertiaryContrastText = tertiary.ToContrastingColor();

        (tertiary, tertiaryContrastText) = EnsureContrast(tertiary, tertiaryContrastText, minContrastRatio);

        MudColor tempColor = backgroundColor;

        if (tempColor.S < 0.5)
        {
            tempColor = tempColor.SetL(tempColor.S + 0.5);
        }
        IEnumerable<MudColor> complementaryColors = tempColor.CreateComplementaryColors(4);

        MudColor info = complementaryColors.First();
        MudColor infoContrastText = info.ToContrastingColor();
        (info, infoContrastText) = EnsureContrast(info, infoContrastText, minContrastRatio);

        MudColor success = complementaryColors.Skip(1).First();
        MudColor successContrastText = success.ToContrastingColor();
        (success, successContrastText) = EnsureContrast(success, successContrastText, minContrastRatio);

        MudColor warning = complementaryColors.Skip(2).First();
        MudColor warningContrastText = warning.ToContrastingColor();
        (warning, warningContrastText) = EnsureContrast(warning, warningContrastText, minContrastRatio);

        MudColor error = complementaryColors.Skip(3).First();
        MudColor errorContrastText = error.ToContrastingColor();
        (error, errorContrastText) = EnsureContrast(error, errorContrastText, minContrastRatio);

        MudColor dark = isLight ? new MudColor(black.H, black.S, 0.1, 255) : new MudColor(white.H, white.S, 0.9, 255);
        MudColor darkContrastText = dark.ToContrastingColor();
        (dark, darkContrastText) = EnsureContrast(dark, darkContrastText, minContrastRatio);

        MudColor disabled = isLight ? new MudColor(black.H, 0.1, 0.35, 255) : new MudColor(white.H, 0.1, 0.65, 255);

        return new TPalette
        {
            White = white,
            Black = black,
            Primary = primary,
            PrimaryContrastText = primaryContrastText,
            Secondary = secondary,
            SecondaryContrastText = secondaryContrastText,
            Tertiary = tertiary,
            TertiaryContrastText = tertiaryContrastText,
            Info = info,
            InfoContrastText = infoContrastText,
            Success = success,
            SuccessContrastText = successContrastText,
            Warning = warning,
            WarningContrastText = warningContrastText,
            Error = error,
            ErrorContrastText = errorContrastText,
            Dark = dark,
            DarkContrastText = darkContrastText,
            TextPrimary = primaryContrastText,
            TextSecondary = secondaryContrastText,
            TextDisabled = disabled,
            Background = backgroundColor,
            BackgroundGray = isLight ? new MudColor(backgroundColor.H, backgroundColor.S, 0.95, 255) : new MudColor(backgroundColor.H, backgroundColor.S, 0.05, 255),
            AppbarBackground = primaryContrastText,
            AppbarText = primary,
            DrawerBackground = secondaryContrastText,
            DrawerText = secondary
        };
    }

    private (MudColor, MudColor) EnsureContrast(MudColor color, MudColor contrastColor, double minContrastRatio)
    {
        int count = 0;
        while (ColorUtils.ContrastRatio(color, contrastColor) < minContrastRatio && count++ < 3)
        {
            if (color.L < 0.5)
            {
                color = color.SetL(color.L + 0.025);
                contrastColor = contrastColor.SetL(contrastColor.L - 0.025);
            }
            else
            {
                color = color.SetL(color.L - 0.025);
                contrastColor = contrastColor.SetL(contrastColor.L + 0.025);
            }
        }

        return (color, contrastColor);
    }

    private double ModifyHue(double backgroundHue, double factor = 0.1) => EnsureHue(backgroundHue + _random.NextDouble() * 360 * factor - 180 * factor);
    private double ModifySaturation(double backgroundSaturation, double factor = 0.1) => EnsureSaturation(backgroundSaturation + _random.NextDouble() * factor - factor / 2);
    private double ModifyLightness(double backgroundLightness, double factor = 0.1) => EnsureSaturation(backgroundLightness + _random.NextDouble() * factor - factor / 2);
    private double FlipLightness(double backgroundLightness, double factor = 0.1) => EnsureLightness(1 - (backgroundLightness + _random.NextDouble() * factor - factor / 2));

    // Is this logic correct? 
    private double Lighten(double lightness, double factor = 0.1)
    {
        if (lightness < 0.5)
        {
            return EnsureLightness(lightness + _random.NextDouble() * factor);
        }
        else
        {
            return EnsureLightness(lightness - _random.NextDouble() * factor);
        }
    }

    // Is this logic correct?
    private double Darken(double lightness, double factor = 0.1)
    {
        if (lightness < 0.5)
        {
            return EnsureLightness(lightness - _random.NextDouble() * factor);
        }
        else
        {
            return EnsureLightness(lightness + _random.NextDouble() * factor);
        }
    }

    private double EnsureHue(double hue) => hue % 360;
    private double EnsureSaturation(double saturation) => Math.Clamp(saturation, 0, 1);
    private double EnsureLightness(double lightness) => Math.Clamp(lightness, 0, 1);

    public EventCallback<MudTheme> OnThemeChanged { get; set; }
}