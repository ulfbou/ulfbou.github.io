using Homepage.Common.Models;
using Homepage.Common.Services;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection.Metadata;

Console.WriteLine("Starting static site generation...");

const string ContentRepoPath = "../../../portfolio-content/";
const string OutputDirectory = "../_site";
const string BaseUrl = "https://ulfbou.github.io/";

var httpClient = new HttpClient();
var markdownService = new MarkdownService(httpClient, null!);

try
{
    if (Directory.Exists(OutputDirectory))
    {
        Console.WriteLine($"Cleaning existing output directory: {OutputDirectory}");
        Directory.Delete(OutputDirectory, true);
    }
    Directory.CreateDirectory(OutputDirectory);
    Directory.CreateDirectory(Path.Combine(OutputDirectory, "content"));

    Console.WriteLine($"Output directory created: {OutputDirectory}");

    var metadataFilePath = Path.Combine(ContentRepoPath, "metadata.json");
    if (!File.Exists(metadataFilePath))
    {
        Console.WriteLine($"Error: metadata.json not found at {metadataFilePath}. Please ensure 'ContentRepoPath' is correct and content is available.");
        return;
    }
    var metadataJson = await File.ReadAllTextAsync(metadataFilePath);
    var allMetadata = JsonSerializer.Deserialize<List<ContentMetadata>>(metadataJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                      ?? new List<ContentMetadata>();

    Console.WriteLine($"Loaded {allMetadata.Count} content metadata entries.");

    string homeHtml = GenerateHtmlPage(
        title: "Ulf's Portfolio - .NET Fullstack Web Developer",
        description: "Hello, I'm Ulf Bourelius, a passionate .NET Fullstack Web Developer and creator of Zentient.Results. Explore my projects, articles, and expertise in Blazor, ASP.NET Core, and Azure.",
        bodyContent: "<div class=\"p-6\"><h1 class=\"mud-typography mud-typography-h4 mb-4\">Hello, I'm Ulf Bourelius!</h1><p class=\"mud-typography mud-typography-body1 mb-6\">Welcome to my portfolio. I'm a passionate and experienced .NET Fullstack Web Developer with a strong focus on building robust, scalable, and maintainable applications using C#, ASP.NET Core, Blazor, and Azure.</p><p class=\"mud-typography mud-typography-body1\">As a fellow developer, explore my technical deep-dives and practical guides.</p></div>"
    );
    await File.WriteAllTextAsync(Path.Combine(OutputDirectory, "index.html"), homeHtml);
    Console.WriteLine("Generated index.html");

    var aboutPageRazorPath = "../../src/Homepage/Pages/About.razor";
    if (!File.Exists(aboutPageRazorPath))
    {
        Console.WriteLine($"Error: About.razor not found at {aboutPageRazorPath}. Skipping About page generation.");
    }
    else
    {
        var aboutContentRaw = await File.ReadAllTextAsync(aboutPageRazorPath);
        var aboutBodyMatch = Regex.Match(aboutContentRaw, @"<MudCardContent>(.*?)</MudCardContent>", RegexOptions.Singleline);
        var aboutBodyHtml = aboutBodyMatch.Success ? aboutBodyMatch.Groups[1].Value : "<h1>About Me</h1><p>Content coming soon.</p>";

        string aboutHtml = GenerateHtmlPage(
            title: "About Ulf Bourelius - .NET Fullstack Developer Portfolio",
            description: "Learn more about Ulf Bourelius, a passionate .NET Fullstack Web Developer, creator of Zentient.Results, and his expertise in Blazor, ASP.NET Core, and Azure.",
            bodyContent: aboutBodyHtml,
            pageUrl: $"{BaseUrl}about"
        );
        await File.WriteAllTextAsync(Path.Combine(OutputDirectory, "about.html"), aboutHtml);
        Console.WriteLine("Generated about.html");
    }

    foreach (var metadata in allMetadata)
    {
        var markdownPath = Path.Combine(ContentRepoPath, metadata.ContentPath);
        if (!File.Exists(markdownPath))
        {
            Console.WriteLine($"Warning: Markdown file not found for {metadata.Slug} at {markdownPath}. Skipping.");
            continue;
        }

        var markdownContent = await File.ReadAllTextAsync(markdownPath);
        var htmlBody = await markdownService.RenderMarkdownToHtmlAsync(markdownContent);

        htmlBody = Regex.Replace(htmlBody, @"^---\s*\n.*?\n---\s*\n", "", RegexOptions.Singleline);
        htmlBody = Regex.Replace(htmlBody, @"@page "".*?""|\@inject .*?|\@using .*?|\@code .*?\s*\{.*?\}", "", RegexOptions.Singleline);

        string fullHtml = GenerateHtmlPage(
            title: $"{metadata.Title} - Ulf's Portfolio",
            description: metadata.Description,
            bodyContent: htmlBody,
            featuredImage: metadata.FeaturedImage,
            pageUrl: $"{BaseUrl}content/{metadata.Slug}",
            tags: metadata.Tags
        );

        var outputPath = Path.Combine(OutputDirectory, "content", $"{metadata.Slug}.html");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, fullHtml);
        Console.WriteLine($"Generated {outputPath}");
    }

    string blazorAppWwwrootDir = "../../src/Homepage/wwwroot";

    if (Directory.Exists(blazorAppWwwrootDir))
    {
        Console.WriteLine($"Copying Blazor app's wwwroot content from {blazorAppWwwrootDir} to {OutputDirectory}...");
        CopyDirectory(blazorAppWwwrootDir, OutputDirectory);
        Console.WriteLine("Finished copying Blazor app's wwwroot content.");
    }
    else
    {
        Console.WriteLine($"Warning: Blazor app wwwroot not found at {blazorAppWwwrootDir}. Static assets might be missing.");
    }

    Console.WriteLine("Static site generation completed successfully!");

}
catch (Exception ex)
{
    Console.WriteLine($"\nAn error occurred during static site generation: {ex.Message}");
    Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
    Environment.ExitCode = 1;
}

// --- Helper Methods ---
static string GenerateHtmlPage(
    string title,
    string description,
    string bodyContent,
    string? featuredImage = null,
    string? pageUrl = null,
    List<string>? tags = null)
{
    string cleanDescription = Regex.Replace(description, @"<[^>]*>", string.Empty);
    cleanDescription = Regex.Replace(cleanDescription, @"\s+", " ").Trim();
    if (cleanDescription.Length > 160) cleanDescription = cleanDescription.Substring(0, 157) + "...";

    string html = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{title}</title>
    <base href=""{BaseUrl}"" />
    <link href=""css/app.css"" rel=""stylesheet"" />
    <link href=""Homepage.styles.css"" rel=""stylesheet"" /> @* Adjusted for Homepage project *@
    <link href=""https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap"" rel=""stylesheet"">
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/atom-one-dark.min.css"">

    <meta name=""description"" content=""{cleanDescription}"" />

    <meta property=""og:type"" content=""article"" />
    <meta property=""og:url"" content=""{pageUrl ?? BaseUrl}"" />
    <meta property=""og:title"" content=""{title}"" />
    <meta property=""og:description"" content=""{cleanDescription}"" />
    {(string.IsNullOrEmpty(featuredImage) ? "" : $"<meta property=\"og:image\" content=\"{featuredImage}\" />")}
    {(tags != null && tags.Any() ? $"<meta property=\"article:tag\" content=\"{string.Join(", ", tags)}\" />" : "")}

    <meta name=""twitter:card"" content=""summary_large_image"" />
    <meta name=""twitter:url"" content=""{pageUrl ?? BaseUrl}"" />
    <meta name=""twitter:title"" content=""{title}"" />
    <meta name=""twitter:description"" content=""{cleanDescription}"" />
    {(string.IsNullOrEmpty(featuredImage) ? "" : $"<meta name=\"twitter:image\" content=\"{featuredImage}\" />")}

    <link rel=""icon"" type=""image/png"" href=""favicon.png"" />
    <link rel=""apple-touch-icon"" sizes=""180x180"" href=""apple-touch-icon.png"">
    <link rel=""icon"" type=""image/png"" sizes=""32x32"" href=""favicon-32x32.png"">
    <link rel=""icon"" type=""image/png"" sizes=""16x16"" href=""favicon-16x16.png"">
    <link rel=""manifest"" href=""site.webmanifest"">
</head>
<body>
    <div id=""app"" style=""display:none;""></div>
    <div class=""d-flex justify-center my-8"" id=""blazor-loading-indicator""><p>Loading interactive content...</p></div>

    <div class=""mud-container mud-container-maxwidth-lg my-8"">
        <div class=""mud-card mud-elevation-4 p-6 rounded-xl"">
            <div class=""mud-card-content markdown-content"">
                {bodyContent}
            </div>
        </div>
    </div>

    <script src=""_framework/blazor.webassembly.js""></script>
    <script src=""js/app.js""></script>
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js""></script>
    <script>
        document.addEventListener(""DOMContentLoaded"", function() {{
            var staticContent = document.querySelector("".markdown-content"");
    if (staticContent)
    {{
        staticContent.querySelectorAll(""pre code"").forEach(function(block) {{
            hljs.highlightElement(block);
        }});
        staticContent.querySelectorAll(""img"").forEach(function(img) {{
            img.setAttribute(""loading"", ""lazy"");
        }});
    }}
}});

document.addEventListener('blazor:started', () => {{
    const staticWrapper = document.querySelector('.mud-container.mud-container-maxwidth-lg.my-8');
    if (staticWrapper)
    {{
        staticWrapper.style.display = 'none';
    }}
    const appRoot = document.getElementById('app');
    if (appRoot)
    {{
        appRoot.style.display = 'block';
    }}
    const loadingIndicator = document.getElementById('blazor-loading-indicator');
    if (loadingIndicator)
    {{
        loadingIndicator.style.display = 'none';
    }}
}});
    </ script>
</ body>
</ html>";
    return html;
}

static void CopyDirectory(string sourceDir, string destinationDir)
{
    var dir = new DirectoryInfo(sourceDir);
    if (!dir.Exists) throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

    DirectoryInfo[] dirs = dir.GetDirectories();
    Directory.CreateDirectory(destinationDir);

    foreach (FileInfo file in dir.GetFiles())
    {
        string targetFilePath = Path.Combine(destinationDir, file.Name);
        file.CopyTo(targetFilePath, true);
    }

    foreach (DirectoryInfo subDir in dirs)
    {
        string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
        CopyDirectory(subDir.FullName, newDestinationDir);
    }
}
