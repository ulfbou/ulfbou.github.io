using Homepage.Common.Extensions;
using Homepage.Common.Services;

using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Utilities;

using Serilog;

namespace Homepage.Layout;

public partial class MainLayout : LayoutComponentBase
{
    [Inject] ThemeManager ThemeManager { get; set; }
    private static readonly Random _random = new Random();
    private static double _hue = _random.NextDouble() * 360;
    private static readonly MudColor White = new MudColor(0.05, _hue, 0.9, 1.0);
    private static readonly MudColor Black = new MudColor(0.05, _hue, 0.1, 1.0);

    public ThemeProperties Properties => _themeProperties;
    private ThemeProperties _themeProperties = new();
    private MudTheme? _theme;

    public ThemeGenerationConfig Config { get; set; } = new ThemeGenerationConfig();
    public MudTheme Theme
    {
        get
        {
            if (_theme == null)
            {
                MudThemeGenerator generator = new();
                _theme = generator.GenerateMonochromeTheme();
                //_theme = ThemeManager.GenerateTheme(_hue);
                //GenerateRandomTheme(Config);
            }
            return _theme!;
        }
    }

    [Inject] required public NavigationManager Navigation { get; set; }

    private bool IsDarkMode { get; set; } = false;

    private ThemeGenerationConfig _config = new();
    private string headerFontFamily = "Playfair Display";
    private string bodyFontFamily = "Lato";

    protected override Task OnInitializedAsync()
    {
        ThemeManager.OnThemeChanged = EventCallback.Factory.Create<MudTheme>(this, OnThemeChanged);
        headerFontFamily = Properties.HeaderFontFamily;
        bodyFontFamily = Properties.BodyFontFamily;
        IsDarkMode = new Random().Next(0, 2) == 0;
        return Task.CompletedTask;
    }

    private void OnThemeChanged(MudTheme theme)
    {
        _theme = theme;
        StateHasChanged();
    }

    private void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        StateHasChanged();
    }

    private bool _drawerOpen = true;

    private void ToggleNavMenu()
    {
        _drawerOpen = !_drawerOpen;
    }

    private MarkupString FontLink => new MarkupString($@" <link rel='stylesheet' href='https://fonts.googleapis.com/css2?family={headerFontFamily}&family={bodyFontFamily}&display=swap'> ");

    public MudTheme GenerateRandomTheme(ThemeGenerationConfig config)
    {
        // Generate a random base color
        var backgroundColor = GenerateRandomColor();

        // Generate light and dark palettes based on the base color

        var colors = config.UseComplementaryColors ? backgroundColor.CreateComplementaryColors(config.ComplementaryColorCount) : new[] { backgroundColor };
        PaletteLight lightPalette = GeneratePalette<PaletteLight>(backgroundColor, false, colors, config);
        PaletteDark darkPalette = GeneratePalette<PaletteDark>(backgroundColor, true, colors, config);

        // Randomly select a pair of completementary font families
        var sansFontFamilies = new[] { "Roboto", "Open Sans", "Arial", "Verdana", "Lato", "Comic Sans MS" };
        var serifFontFamilies = new[] { "Times New Roman", "Georgia", "Palatino Linotype", "Book Antiqua", "Palatino", "Lucida Bright" };

        var headerFontFamilies = Properties.HeaderFontFamily;
        var bodyFontFamilies = Properties.BodyFontFamily;

        // Randomize additional theme properties

        var shadowIntensity = _random.Next(1, 5);

        _theme = new MudTheme
        {
            PaletteLight = lightPalette,
            PaletteDark = darkPalette,
            Typography = new Typography
            {
                Default = new Default { FontFamily = [bodyFontFamilies], FontSize = "12px" },
                Body1 = new Body1 { FontFamily = [bodyFontFamilies], FontSize = "1rem" },
                Body2 = new Body2 { FontFamily = [bodyFontFamilies], FontSize = "0.875rem" },
                Caption = new Caption { FontFamily = [bodyFontFamilies], FontSize = "0.75rem" },
                Subtitle1 = new Subtitle1 { FontFamily = [bodyFontFamilies], FontSize = "1rem" },
                Subtitle2 = new Subtitle2 { FontFamily = [bodyFontFamilies], FontSize = "0.875rem" },
                Button = new Button { FontFamily = [headerFontFamilies], FontSize = "0.875rem" },
                H1 = new H1 { FontFamily = [headerFontFamilies], FontSize = "2rem" },
                H2 = new H2 { FontFamily = [headerFontFamilies], FontSize = "1.5rem" },
                H3 = new H3 { FontFamily = [headerFontFamilies], FontSize = "1.25rem" },
                H4 = new H4 { FontFamily = [headerFontFamilies], FontSize = "1.125rem" },
                H5 = new H5 { FontFamily = [headerFontFamilies], FontSize = "1rem" },
                H6 = new H6 { FontFamily = [headerFontFamilies], FontSize = "0.875rem" },
                Overline = new Overline { FontFamily = [headerFontFamilies], FontSize = "0.75rem" }
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = $"{_random.Next(4, 10)}px",
                DrawerWidthLeft = $"{_random.Next(200, 300)}px"
            }
        };

        return Theme;
    }

    // Generate a random color
    private MudColor GenerateRandomColor()
    {
        // Generate a random color with good contrast
        var hue = _random.NextDouble() * 360;
        var saturation = _random.NextDouble() * 0.8 + 0.2; // Adjust saturation range as needed
        var lightness = _random.NextDouble() * 0.5 + 0.25; // Adjust lightness range as needed

        return new MudColor(hue, saturation, lightness, 255);
    }

    private TPalette GeneratePalette<TPalette>(MudColor baseColor, bool isDarkMode = false, IEnumerable<MudColor> colors = null, ThemeGenerationConfig config = null) where TPalette : Palette, new()
    {
        // Determine the base color for the palette
        var baseHue = baseColor.H;
        var baseSaturation = baseColor.S;
        var baseValue = baseColor.L;

        // Generate a complementary color
        var complementaryHue = (baseHue + 180) % 360;
        var complementaryColor = new MudColor(complementaryHue, baseSaturation, baseValue, 255);

        // Choose the primary and secondary colors based on contrast
        var primaryColor = baseColor.GetContrastRatio(White) > baseColor.GetContrastRatio(Black) ? baseColor : complementaryColor;
        var secondaryColor = primaryColor == baseColor ? complementaryColor : baseColor;

        // Choose a unsaturated color for the tertiary color
        var tertiaryColor = new MudColor(baseHue, 0.1, baseValue, 255);

        // Determine the text color based on the primary color's lightness
        var textColor = primaryColor.L > 0.5 ? Black : White;

        // Generate lighter and darker shades for primary and secondary colors
        var primaryLight = primaryColor.ColorLighten(0.1);
        var primaryDark = primaryColor.ColorDarken(0.1);
        var secondaryLight = secondaryColor.ColorLighten(0.1);
        var secondaryDark = secondaryColor.ColorDarken(0.1);
        // Handle missing configuration
        config ??= new ThemeGenerationConfig();

        var (hue, saturation, lightness) = baseColor.ToHsl();
        complementaryHue = (hue + 180) % 360;

        // Generate complementary colors (if desired)
        complementaryColor = colors?.FirstOrDefault() ?? new MudColor(complementaryHue, baseColor.S, baseColor.L, 255);

        // Choose the primary and secondary colors based on contrast
        if (isDarkMode)
        {
            lightness = _random.NextDouble() * 0.1 + 0.1;
        }
        else
        {
            lightness = 0.9 - _random.NextDouble() * 0.1;
        }
        var backgroundColor = new MudColor(hue, saturation, lightness, 255);
        if (backgroundColor.GetContrastRatio(White) > backgroundColor.GetContrastRatio(Black))
        {
            primaryColor = backgroundColor;
            secondaryColor = complementaryColor;
        }
        else
        {
            primaryColor = complementaryColor;
            secondaryColor = backgroundColor;
        }

        Log.Information("Background Color: {BackgroundColor} Primary color: {PrimaryColor}, Secondary color: {SecondaryColor}", backgroundColor, primaryColor, secondaryColor);

        // Determine the text color based on the primary color's lightness
        textColor = primaryColor.L > 0.5 ? Black : White;

        // Generate lighter and darker shades for primary and secondary colors
        primaryLight = primaryColor.ColorLighten(config.PrimaryColorLightnessRange);
        primaryDark = primaryColor.ColorDarken(config.PrimaryColorLightnessRange);
        secondaryLight = secondaryColor.ColorLighten(config.SecondaryColorLightnessRange);
        secondaryDark = secondaryColor.ColorDarken(config.SecondaryColorLightnessRange);

        // Populate palette properties
        return new TPalette
        {
            Black = Black,
            White = White,
            Primary = primaryColor,
            PrimaryContrastText = textColor,
            Secondary = secondaryColor,
            SecondaryContrastText = textColor,
            Background = backgroundColor,
            Surface = _random.Next(2) == 0 ? backgroundColor.ColorDarken(config.SurfaceColorLightnessRange * 0.8) : backgroundColor.ColorLighten(config.SurfaceColorLightnessRange * 0.8),
            Tertiary = primaryLight,
            TertiaryContrastText = textColor,
            Info = secondaryLight,
            InfoContrastText = textColor,
            Success = primaryDark,
            SuccessContrastText = textColor,
            Warning = secondaryDark,
            WarningContrastText = textColor,
            Error = Black,
            ErrorContrastText = White,
            Dark = Black,
            DarkContrastText = White,
            TextPrimary = textColor,
            TextSecondary = textColor,
            TextDisabled = textColor.ColorDarken(0.2),
            ActionDefault = secondaryColor,
            ActionDisabled = secondaryColor.ColorDarken(0.2),
            ActionDisabledBackground = secondaryColor.ColorLighten(0.2),
            BackgroundGray = baseColor.ColorLighten(0.1),
            DrawerBackground = secondaryLight,
            DrawerText = textColor,
            DrawerIcon = textColor,
            AppbarBackground = primaryDark,
            AppbarText = textColor,
            LinesDefault = baseColor.ColorLighten(0.1),
            LinesInputs = baseColor.ColorLighten(0.1),
            TableLines = baseColor.ColorLighten(0.1),
            TableStriped = baseColor.ColorLighten(0.1),
            TableHover = baseColor.ColorLighten(0.1),
            Divider = baseColor.ColorLighten(0.1),
            DividerLight = baseColor.ColorLighten(0.1),
            Skeleton = baseColor.ColorLighten(0.1),
            PrimaryDarken = primaryDark.ToString(),
            PrimaryLighten = primaryLight.ToString(),
            SecondaryDarken = secondaryDark.ToString(),
            SecondaryLighten = secondaryLight.ToString(),
            TertiaryDarken = primaryDark.ToString(),
            TertiaryLighten = primaryLight.ToString(),
            InfoDarken = secondaryDark.ToString(),
            InfoLighten = secondaryLight.ToString(),
            SuccessDarken = primaryDark.ToString(),
            SuccessLighten = primaryLight.ToString(),
            WarningDarken = secondaryDark.ToString(),
            WarningLighten = secondaryLight.ToString(),
            ErrorDarken = Black.ToString(),
            ErrorLighten = White.ToString(),
            DarkDarken = Black.ToString(),
            DarkLighten = White.ToString(),
            HoverOpacity = 0.08,
            RippleOpacity = 0.08,
            RippleOpacitySecondary = 0.08,
            GrayDefault = baseColor.ColorLighten(0.1).ToString(),
            GrayLight = baseColor.ColorLighten(0.2).ToString(),
            GrayLighter = baseColor.ColorLighten(0.3).ToString(),
            GrayDark = baseColor.ColorDarken(0.1).ToString(),
            GrayDarker = baseColor.ColorDarken(0.2).ToString(),
            OverlayDark = baseColor.ColorDarken(0.5).ToString(),
            OverlayLight = baseColor.ColorLighten(0.5).ToString()
        };
    }

    private TPalette GeneratePalette<TPalette>(MudColor baseColor) where TPalette : Palette, new()
    {
        // Determine the base color for the palette
        var baseHue = baseColor.H;
        var baseSaturation = baseColor.S;
        var baseValue = baseColor.L;

        // Generate a complementary color
        var complementaryHue = (baseHue + 180) % 360;
        var complementaryColor = new MudColor(complementaryHue, baseSaturation, baseValue, 255);

        // Choose the primary and secondary colors based on contrast
        var primaryColor = baseColor.GetContrastRatio(White) > baseColor.GetContrastRatio(Black) ? baseColor : complementaryColor;
        var secondaryColor = primaryColor == baseColor ? complementaryColor : baseColor;

        // Choose a unsaturated color for the tertiary color
        var tertiaryColor = new MudColor(baseHue, 0.1, baseValue, 255);

        // Determine the text color based on the primary color's lightness
        var textColor = primaryColor.L > 0.5 ? Black : White;

        // Generate lighter and darker shades for primary and secondary colors
        var primaryLight = primaryColor.ColorLighten(0.1);
        var primaryDark = primaryColor.ColorDarken(0.1);
        var secondaryLight = secondaryColor.ColorLighten(0.1);
        var secondaryDark = secondaryColor.ColorDarken(0.1);

        return new TPalette
        {
            Black = Black,
            White = White,
            Primary = primaryColor,
            PrimaryContrastText = textColor,
            Secondary = secondaryColor,
            SecondaryContrastText = textColor,
            Tertiary = primaryLight,
            TertiaryContrastText = textColor,
            Info = secondaryLight,
            InfoContrastText = textColor,
            Success = primaryDark,
            SuccessContrastText = textColor,
            Warning = secondaryDark,
            WarningContrastText = textColor,
            Error = Black,
            ErrorContrastText = White,
            Dark = Black,
            DarkContrastText = White,
            TextPrimary = textColor,
            TextSecondary = textColor,
            TextDisabled = textColor.ColorDarken(0.2),
            ActionDefault = secondaryColor,
            ActionDisabled = secondaryColor.ColorDarken(0.2),
            ActionDisabledBackground = secondaryColor.ColorLighten(0.2),
            Background = baseColor,
            BackgroundGray = baseColor.ColorLighten(0.1),
            Surface = baseColor.ColorLighten(0.2),
            DrawerBackground = secondaryLight,
            DrawerText = textColor,
            DrawerIcon = textColor,
            AppbarBackground = primaryDark,
            AppbarText = textColor,
            LinesDefault = baseColor.ColorLighten(0.1),
            LinesInputs = baseColor.ColorLighten(0.1),
            TableLines = baseColor.ColorLighten(0.1),
            TableStriped = baseColor.ColorLighten(0.1),
            TableHover = baseColor.ColorLighten(0.1),
            Divider = baseColor.ColorLighten(0.1),
            DividerLight = baseColor.ColorLighten(0.1),
            Skeleton = baseColor.ColorLighten(0.1),
            PrimaryDarken = primaryDark.ToString(),
            PrimaryLighten = primaryLight.ToString(),
            SecondaryDarken = secondaryDark.ToString(),
            SecondaryLighten = secondaryLight.ToString(),
            TertiaryDarken = primaryDark.ToString(),
            TertiaryLighten = primaryLight.ToString(),
            InfoDarken = secondaryDark.ToString(),
            InfoLighten = secondaryLight.ToString(),
            SuccessDarken = primaryDark.ToString(),
            SuccessLighten = primaryLight.ToString(),
            WarningDarken = secondaryDark.ToString(),
            WarningLighten = secondaryLight.ToString(),
            ErrorDarken = Black.ToString(),
            ErrorLighten = White.ToString(),
            DarkDarken = Black.ToString(),
            DarkLighten = White.ToString(),
            HoverOpacity = 0.08,
            RippleOpacity = 0.08,
            RippleOpacitySecondary = 0.08,
            GrayDefault = baseColor.ColorLighten(0.1).ToString(),
            GrayLight = baseColor.ColorLighten(0.2).ToString(),
            GrayLighter = baseColor.ColorLighten(0.3).ToString(),
            GrayDark = baseColor.ColorDarken(0.1).ToString(),
            GrayDarker = baseColor.ColorDarken(0.2).ToString(),
            OverlayDark = baseColor.ColorDarken(0.5).ToString(),
            OverlayLight = baseColor.ColorLighten(0.5).ToString()
        };
    }

    public class ThemeProperties
    {
        public bool IsDarkMode { get; set; }
        public int ShadowIntensity { get; set; }
        public string HeaderFontFamily { get; set; }
        public string BodyFontFamily { get; set; }

        public ThemeProperties()
        {
            Randomize();
        }

        public void Randomize()
        {
            var fontFamilyIndex = _random.Next(FontPairs.Length);

            IsDarkMode = _random.Next(2) == 0;
            ShadowIntensity = _random.Next(1, 5);
            HeaderFontFamily = FontPairs[fontFamilyIndex].Item1;
            BodyFontFamily = FontPairs[fontFamilyIndex].Item2;
        }

        private (string, string)[] FontPairs = {
            ("Playfair Display", "Source Sans Pro"),
            ("Quattrocento", "Quattrocento Sans"),
            ("Libre Baskerville", "Source Sans Pro"),
            ("Lora", "Open Sans"),
            ("Merriweather", "Raleway"),
            ("Oswald", "Quattrocento"),
            ("Fjalla One", "Libre Baskerville"),
            ("Cormorant Garamond", "Proza Libre"),
            ("Abril Fatface", "Work Sans"),
            ("Roboto", "Roboto Slab"),
            ("Montserrat", "Raleway"),
            ("Lustria", "Lato"),
            ("Arvo", "Roboto"),
            ("Bebas Neue", "Source Sans Pro"),
            ("IBM Plex Sans", "IBM Plex Serif"),
            ("Nunito", "Merriweather"),
            ("Poppins", "Roboto"),
            ("PT Sans", "PT Serif"),
            ("Playfair Display", "Montserrat"),
            ("Spectral", "Roboto Mono"),
            ("Crimson Text", "Work Sans"),
            ("Alegreya", "Alegreya Sans"),
            ("Noto Serif", "Noto Sans"),
            ("Cardo", "Josefin Sans"),
            ("Vollkorn", "Open Sans")
       };
    }

    public class ThemeGenerationConfig
    {
        public double PrimaryColorLightnessRange { get; set; }
        public double SecondaryColorLightnessRange { get; set; }
        public double SurfaceColorLightnessRange { get; set; }
        public int ComplementaryColorCount { get; set; }
        public bool UseComplementaryColors { get; set; }
    }
}