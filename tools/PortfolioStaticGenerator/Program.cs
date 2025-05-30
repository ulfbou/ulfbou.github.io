using Homepage.Common.Models;
using Homepage.Common.Services;

using Microsoft.Extensions.Configuration;

using Serilog;

using System.Text.Json;
using System.Text.RegularExpressions;

public class Program
{
    public static async Task Main(string[] args)
    {
        // 1. Configuration Setup: Get values from command line arguments
        var builder = new ConfigurationBuilder();
        builder.AddCommandLine(args);
        var configuration = builder.Build();

        // Get paths from command line. Provide fallback defaults for local development.
        string contentRepoPath = configuration["ContentRepoPath"] ?? "./portfolio-content";
        string outputDirectory = configuration["OutputDirectory"] ?? "../_site";
        string baseUrl = configuration["BaseUrl"] ?? "https://ulfbou.github.io/";
        // New: Get the path to the published Blazor app's static assets
        string blazorPublishOutputPath = configuration["BlazorPublishOutput"] ?? "./src/Homepage/wwwroot"; // Fallback to source wwwroot for local dev if not published

        // Logging setup
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.Console()
            .CreateLogger();

        Log.Information("Starting static site generation...");

        var httpClient = new HttpClient();
        var markdownService = new MarkdownService(httpClient, null!);

        try
        {
            // 2. Clean and Create Output Directory
            if (Directory.Exists(outputDirectory))
            {
                Log.Information($"Cleaning existing output directory: {outputDirectory}");
                Directory.Delete(outputDirectory, true);
            }

            Directory.CreateDirectory(outputDirectory);
            Directory.CreateDirectory(Path.Combine(outputDirectory, "content"));

            Log.Information($"Output directory created: {outputDirectory}");

            // 3. Load Metadata
            var metadataFilePath = Path.Combine(contentRepoPath, "metadata.json");
            if (!File.Exists(metadataFilePath))
            {
                Log.Error($"Error: metadata.json not found at {metadataFilePath}. Please ensure 'ContentRepoPath' is correct and content is available.");
                Environment.Exit(1);
            }
            var metadataJson = await File.ReadAllTextAsync(metadataFilePath);
            var allMetadata = JsonSerializer.Deserialize<List<ContentMetadata>>(metadataJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                              ?? new List<ContentMetadata>();

            Log.Information($"Loaded {allMetadata.Count} content metadata entries.");

            // 4. Generate Home Page
            string homeHtml = GenerateHtmlPage(
                title: "Ulf's Portfolio - .NET Fullstack Web Developer",
                description: "Hello, I'm Ulf Bourelius, a passionate .NET Fullstack Web Developer and creator of Zentient.Results. Explore my projects, articles, and expertise in Blazor, ASP.NET Core, and Azure.",
                bodyContent: "<div class=\"p-6\"><h1 class=\"mud-typography mud-typography-h4 mb-4\">Hello, I'm Ulf Bourelius!</h1><p class=\"mud-typography mud-typography-body1 mb-6\">Welcome to my portfolio. I'm a passionate and experienced .NET Fullstack Web Developer with a strong focus on building robust, scalable, and maintainable applications using C#, ASP.NET Core, Blazor, and Azure.</p><p class=\"mud-typography mud-typography-body1\">As a fellow developer, explore my technical deep-dives and practical guides.</p></div>",
                baseUrl: baseUrl
            );
            await File.WriteAllTextAsync(Path.Combine(outputDirectory, "index.html"), homeHtml);
            Log.Information("Generated index.html");

            // 5. Generate About Page (Adjust pathing for src/Homepage)
            string aboutPageRazorPath = "./src/Homepage/Pages/About.razor"; // Relative to main repo root
            if (!File.Exists(aboutPageRazorPath))
            {
                Log.Warning($"About.razor not found at {aboutPageRazorPath}. Skipping About page generation. Ensure it's relative to the main repo's root.");
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
                    pageUrl: $"{baseUrl}about",
                    baseUrl: baseUrl
                );
                await File.WriteAllTextAsync(Path.Combine(outputDirectory, "about.html"), aboutHtml);
                Log.Information("Generated about.html");
            }

            // 6. Generate Content Pages
            foreach (var metadata in allMetadata)
            {
                var markdownPath = Path.Combine(contentRepoPath, metadata.ContentPath);
                if (!File.Exists(markdownPath))
                {
                    Log.Warning($"Markdown file not found for {metadata.Slug} at {markdownPath}. Skipping.");
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
                    pageUrl: $"{baseUrl}content/{metadata.Slug}",
                    tags: metadata.Tags,
                    baseUrl: baseUrl
                );

                var outputPath = Path.Combine(outputDirectory, "content", $"{metadata.Slug}.html");
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                await File.WriteAllTextAsync(outputPath, fullHtml);
                Log.Information($"Generated {outputPath}");
            }

            // 7. Copy Blazor App's static assets from its publish output
            // This is the crucial part to get all framework and component library assets
            if (Directory.Exists(blazorPublishOutputPath))
            {
                Log.Information($"Copying Blazor app's static content from {blazorPublishOutputPath} to {outputDirectory}...");
                CopyDirectory(blazorPublishOutputPath, outputDirectory);
                Log.Information("Finished copying Blazor app's static content.");
            }
            else
            {
                Log.Warning($"Blazor app publish output not found at {blazorPublishOutputPath}. Static assets might be missing.");
            }

            Log.Information("Static site generation completed successfully!");

        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during static site generation.");
            Environment.ExitCode = 1; // Indicate failure
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    static string GenerateHtmlPage(
            string title,
            string description,
            string bodyContent,
            string? featuredImage = null,
            string? pageUrl = null,
            List<string>? tags = null,
            string baseUrl = "https://ulfbou.github.io/")
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
    <base href=""{baseUrl}"" />
    <link href=""css/app.css"" rel=""stylesheet"" />
    <link href=""Homepage.styles.css"" rel=""stylesheet"" /> @* Adjusted for Homepage project *@
    <link href=""https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap"" rel=""stylesheet"">
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/atom-one-dark.min.css"">

    <meta name=""description"" content=""{cleanDescription}"" />

    <meta property=""og:type"" content=""article"" />
    <meta property=""og:url"" content=""{pageUrl ?? baseUrl}"" />
    <meta property=""og:title"" content=""{title}"" />
    <meta property=""og:description"" content=""{cleanDescription}"" />
    {(string.IsNullOrEmpty(featuredImage) ? "" : $"<meta property=\"og:image\" content=\"{featuredImage}\" />")}
    {(tags != null && tags.Any() ? $"<meta property=\"article:tag\" content=\"{string.Join(", ", tags)}\" />" : "")}

    <meta name=""twitter:card"" content=""summary_large_image"" />
    <meta name=""twitter:url"" content=""{pageUrl ?? baseUrl}"" />
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
}
