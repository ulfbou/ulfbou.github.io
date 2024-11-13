using MudBlazor;
using MudBlazor.Utilities;

namespace Homepage.Common.Extensions;

public static class MudColorExtensions
{
    private static Random Random = new Random();
    public static MudColor FromAhsv(this MudColor color, byte a, double h, double s, double v)
    {
        // Convert HSV to HSL
        double l = v - (v * s) / 2;
        double m = l + (v * s) * (1 - Math.Abs((h / 60) % 2 - 1));
        double c = v - m;

        // Convert HSL to RGB
        int r = (int)Math.Round((c * (1 + (h < 180 ? (h / 60 - 1) : (1 - h / 180))) + m) * 255);
        int g = (int)Math.Round((c * (1 + (h < 300 ? (h / 180 - 1) : (3 - h / 180))) + m) * 255);
        int b = (int)Math.Round((c * (1 + (h < 240 ? (h / 180 + 1) : (3 - h / 180))) + m) * 255);

        return new MudColor(r, g, b, a);
    }

    public static double GetContrastRatio(this MudColor color1, MudColor color2)
    {
        double luminance1 = (0.2126 * color1.R + 0.7152 * color1.G + 0.0722 * color1.B) / 255;
        double luminance2 = (0.2126 * color2.R + 0.7152 * color2.G + 0.0722 * color2.B) / 255;

        double lighterLuminance = Math.Max(luminance1, luminance2);
        double darkerLuminance = Math.Min(luminance1, luminance2);

        return (lighterLuminance + 0.05) / (darkerLuminance + 0.05);
    }
    public static (double H, double S, double L) ToHsl(this MudColor color)
    {
        // Convert RGB to HSL
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = Math.Max(Math.Max(r, g), b);
        double min = Math.Min(Math.Min(r, g), b);
        double h, s, l;

        l = (max + min) / 2;

        if (max == min)
        {
            h = s = 0; // achromatic
        }
        else
        {
            double delta = max - min;
            s = l > 0.5 ? delta / (2 - max - min) : delta / (max + min);

            if (max == r)
            {
                h = (g - b) / delta;
            }
            else if (max == g)
            {
                h = 2 + (b - r) / delta;
            }
            else
            {
                h = 4 + (r - g) / delta;
            }

            h *= 60;
            if (h < 0)
            {
                h += 360;
            }
        }

        return (h, s, l);
    }

    public static MudColor FromRanges(this MudColor color, double hueRange = 0.1, int maxHue = 360, double saturationRange = 0.8, double lightnessRange = 0.5, int alpha = 255)
    {
        var hue = Random.NextDouble() * hueRange - hueRange / 2;
        var saturation = Random.NextDouble() * saturationRange + (1 - saturationRange) / 2;
        var lightness = Random.NextDouble() * lightnessRange + (1 - lightnessRange) / 2;
        return new MudColor(hue, saturation, lightness, alpha);
    }

    public static MudColor ToContrastingColor(this MudColor color, double hueRange = 0.1, double saturationRange = 0.2, double lightnessRange = 0.3)
    {
        var hue = EnsureRange(color.H + Random.NextDouble() * hueRange - hueRange / 2);
        var saturation = EnsureRange(color.S + Random.NextDouble() * saturationRange - saturationRange / 2);
        var lightness = EnsureRange(1 - color.L + Random.NextDouble() * lightnessRange / 2);

        if (lightness > 0.3 && lightness < 0.7)
        {
            if (lightness < 0.5)
            {
                lightness = 0.1;
            }
            else
            {
                lightness = 0.9;
            }
        }

        return new MudColor(hue, saturation, lightness, color.A);
    }

    public static IEnumerable<MudColor> CreateComplementaryColors(this MudColor color, int number = 2)
    {
        if (number > 0)
        {
            var complementaryHue = color.H;
            var complementaryDistance = 1.0 / number;
            for (int i = 0; i < number; i++)
            {
                var complementaryColor = new MudColor(complementaryHue, color.S, color.L, color.A);
                complementaryHue = (complementaryHue + complementaryDistance) % 360;
                yield return complementaryColor;
            }
        }
    }

    private static double EnsureHue(double value) => EnsureRange(value);
    private static double EnsureRange(double value) => Math.Min(1, Math.Max(0, value));
    private static int EnsureRange(int value) => Math.Min(255, Math.Max(0, value));

    public static bool IsLight(this MudColor color)
    {
        return color.L >= 0.7;
    }

    public static bool IsDark(this MudColor color)
    {
        return color.L <= 0.3;
    }
}