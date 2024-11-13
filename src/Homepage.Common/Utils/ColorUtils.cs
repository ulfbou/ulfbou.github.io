using MudBlazor.Utilities;

namespace Homepage.Common.Utils;

public static class ColorUtils
{
    public static double ContrastRatio(MudColor color, MudColor contrastColor)
    {
        double luminance1 = (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255;
        double luminance2 = (0.2126 * contrastColor.R + 0.7152 * contrastColor.G + 0.0722 * contrastColor.B) / 255;

        double lighterLuminance = Math.Max(luminance1, luminance2);
        double darkerLuminance = Math.Min(luminance1, luminance2);

        return (lighterLuminance + 0.05) / (darkerLuminance + 0.05);
    }
}
