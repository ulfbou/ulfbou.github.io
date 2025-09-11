using Homepage.Common.Models;
using Homepage.Common.Services;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Serilog;

namespace Homepage.Layout
{
	public partial class MainLayout : LayoutComponentBase
	{
		private MudThemeProvider _mudThemeProvider = new MudThemeProvider();
		private bool _drawerOpen = true;
		private bool _isDarkMode = false;

		[Inject] public FilterService SearchService { get; set; } = default!;

		private string SearchText { get; set; } = string.Empty;

		protected override async Task OnInitializedAsync()
		{
			var logger = Log.Logger.ForContext<MainLayout>()
				.ForContext("Page", NavManager.Uri)
				.ForContext("Audience", AudienceService != null ? AudienceService.CurrentAudience : "null");

			logger.Debug("Initializing MainLayout");

			_mudThemeProvider = new MudThemeProvider();
			_isDarkMode = _mudThemeProvider.IsDarkMode;
			await base.OnInitializedAsync();
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				if (_mudThemeProvider != null)
				{
					await _mudThemeProvider.WatchSystemPreference(OnSystemPreferenceChanged);
				}
				StateHasChanged();
			}
		}

		private Task OnSystemPreferenceChanged(bool isDarkMode)
		{
			_isDarkMode = isDarkMode;
			return Task.CompletedTask;
		}

		private void ToggleNavMenu()
		{
			_drawerOpen = !_drawerOpen;
		}

		private void ToggleDarkMode()
		{
			_isDarkMode = !_isDarkMode;
			StateHasChanged();
		}

		private void NavigateHome()
		{
			NavManager.NavigateTo("/");
		}

		public MudTheme CustomTheme = new MudTheme
		{
			Typography = new Typography
			{
				Default = new Default
				{
					FontFamily = new[] { "Lato", "sans-serif" },
					FontWeight = 300,
					FontSize = "1rem",
					LineHeight = 1.5,
					LetterSpacing = "0.00938em",
				},
				H1 = new H1
				{
					FontFamily = new[] { "Playfair Display", "Oranienbaum", "serif" },
					FontSize = "2.5rem",
					FontWeight = 700,
					LineHeight = 1.2,
					LetterSpacing = "-0.01562em",
				},
				H2 = new H2
				{
					FontFamily = new[] { "Playfair Display", "Oranienbaum", "serif" },
					FontSize = "2rem",
					FontWeight = 700,
					LineHeight = 1.2,
					LetterSpacing = "-0.00833em",
				},
				H3 = new H3
				{
					FontFamily = new[] { "Playfair Display", "Oranienbaum", "serif" },
					FontSize = "1.75rem",
					FontWeight = 700,
					LineHeight = 1.2,
					LetterSpacing = "0em",
				},
				H4 = new H4
				{
					FontFamily = new[] { "Oranienbaum", "serif" },
					FontSize = "1.5rem",
					FontWeight = 700,
					LineHeight = 1.2,
					LetterSpacing = "0.00735em",
				},
				H5 = new H5
				{
					FontFamily = new[] { "Oranienbaum", "serif" },
					FontSize = "1.25rem",
					FontWeight = 700,
					LineHeight = 1.2,
					LetterSpacing = "0em",
				},
				H6 = new H6
				{
					FontFamily = new[] { "Oranienbaum", "serif" },
					FontSize = "1rem",
					FontWeight = 700,
					LineHeight = 1.2,
					LetterSpacing = "0.00938em",
				},
				Body1 = new Body1
				{
					FontFamily = new[] { "Lato", "sans-serif" },
					FontSize = "0.875rem",
					FontWeight = 400,
					LineHeight = 1.43,
					LetterSpacing = "0.01071em",
				},
				Body2 = new Body2
				{
					FontFamily = new[] { "Droid Sans", "sans-serif" },
					FontSize = "0.875rem",
					FontWeight = 400,
					LineHeight = 1.43,
					LetterSpacing = "0.01071em",
				},
				Subtitle1 = new Subtitle1
				{
					FontFamily = new[] { "Lato", "sans-serif" },
					FontSize = "1rem",
					FontWeight = 400,
					LineHeight = 1.75,
					LetterSpacing = "0.00938em",
				},
				Subtitle2 = new Subtitle2
				{
					FontFamily = new[] { "Droid Sans", "sans-serif" },
					FontSize = "0.875rem",
					FontWeight = 500,
					LineHeight = 1.57,
					LetterSpacing = "0.00714em",
				}
			},
			PaletteLight = new PaletteLight
			{
				Primary = "#3E2723",
				Secondary = "#A1887F",
				AppbarBackground = "#6D4C41",
				Background = "#EEEEDE",
				Surface = "#F2F2F8",
				DrawerBackground = "#D7CCC8",
				DrawerText = "#3E2723",
				TextPrimary = "#3E2723",
				TextSecondary = "#5D4037",
				ActionDefault = "#3E2723",
				ActionDisabled = "#BCAAA4",
				ActionDisabledBackground = "#E0E0E0",
				Divider = "#CCCCCC",
				DividerLight = "#E0E0E0",
				TableLines = "#CCCCCC",
				LinesDefault = "#CCCCCC",
				LinesInputs = "#B0B0B0",
				TextDisabled = "#BCAAA4",
				Info = "#4A90E2",
				Success = "#50E3C2",
				Warning = "#F5A623",
				Error = "#D0021B",
				Dark = "#3E2723"
			}

		};
	}
}