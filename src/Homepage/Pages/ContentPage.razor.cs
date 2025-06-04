using Homepage.Common.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Serilog;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Homepage.Pages
{
	public partial class ContentPage : Homepage.Components.Base.ContentBase
	{
		[Parameter]
		public string Slug { get; set; } = string.Empty;
		private string? _htmlContent;
		private string? _tocHtmlContent;
		private bool _isLoading = true;
		private ContentMetadata? _currentMetadata;
		private ElementReference _markdownContentContainer;
		private List<TocEntry> _tocEntries = new();

		/// <inheritdoc />
		protected override async Task OnParametersSetAsync()
		{
			_isLoading = true;
			_htmlContent = null;
			_currentMetadata = null;
			_tocHtmlContent = null;
			_tocEntries.Clear();

			var allMetadata = await MarkdownService.GetContentMetadataAsync();
			_currentMetadata = allMetadata.FirstOrDefault(m => m.Slug.Equals(Slug, StringComparison.OrdinalIgnoreCase));

			if (_currentMetadata != null)
			{
				var markdown = await MarkdownService.GetMarkdownContentAsync(_currentMetadata.ContentPath);
				(string mainHtml, string generatedTocHtml) = await MarkdownService.RenderMarkdownWithTocAsync(markdown);
				_htmlContent = mainHtml;
				_tocHtmlContent = generatedTocHtml;
				_tocEntries = ParseTocHtmlToTocEntries(generatedTocHtml);
			}
			else
			{
				_htmlContent = null;
				_tocHtmlContent = null;
				_tocEntries.Clear();
			}

			Log.ForContext("Class: {Name}", GetType().Name)
			   .ForContext("Method", "OnParametersSetAsync")
			   .ForContext("Slug", Slug)
			   .ForContext("TocEntriesCount", _tocEntries.Count)
			   .Debug("ContentPage.razor.cs: Loaded content for slug '{Slug}'. Metadata found: {MetadataFound}. TOC entries found: {TocEntriesCount}", Slug, _currentMetadata != null, _tocEntries.Count);

			_isLoading = false;
		}

		/// <inheritdoc />
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (!_isLoading && _htmlContent != null && _markdownContentContainer.Id != null)
			{
				await JSRuntime.InvokeVoidAsync("appJsFunctions.highlightCode", _markdownContentContainer.Id);
				await JSRuntime.InvokeVoidAsync("appJsFunctions.applyLazyLoading", _markdownContentContainer.Id);
			}
		}

		private List<TocEntry> ParseTocHtmlToTocEntries(string tocHtml)
		{
			var entries = new List<TocEntry>();

			if (string.IsNullOrWhiteSpace(tocHtml))
			{
				return entries;
			}

			var listItemRegex = new Regex(@"<li class=""toc-level-(\d+)""><a href=""#(.*?)"">(.*?)</a></li>", RegexOptions.Singleline);

			foreach (Match match in listItemRegex.Matches(tocHtml))
			{
				if (int.TryParse(match.Groups[1].Value, out int level))
				{
					entries.Add(new TocEntry
					{
						Id = match.Groups[2].Value,
						Text = match.Groups[3].Value,
						Level = level
					});
				}
			}

			return entries;
		}

		private async Task ScrollToHeading(string id)
			=> await JSRuntime.InvokeVoidAsync("appJsFunctions.scrollToElement", id);
	}
}
