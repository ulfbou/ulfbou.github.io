
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ulfbou.Portfolio.Core;

public sealed class ApprovedContentLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public ApprovedPortfolio Load(string contentDirectory)
    {
        if (string.IsNullOrWhiteSpace(contentDirectory))
        {
            throw new ContentValidationException(
                "LOAD-DIRECTORY",
                "<null>",
                "Content directory was not specified.");
        }

        var fullDirectory = Path.GetFullPath(contentDirectory);

        if (!Directory.Exists(fullDirectory))
        {
            throw new ContentValidationException(
                "LOAD-DIRECTORY",
                fullDirectory,
                "Content directory does not exist.");
        }

        var profilePath = RequiredFile(
            fullDirectory,
            "profile.json");

        var facetsPath = RequiredFile(
            fullDirectory,
            "facets.json");

        var technologiesPath = RequiredFile(
            fullDirectory,
            "technologies.json");

        var projectsDirectory = Path.Combine(
            fullDirectory,
            "projects");

        if (!Directory.Exists(projectsDirectory))
        {
            throw new ContentValidationException(
                "LOAD-DIRECTORY",
                projectsDirectory,
                "Projects directory does not exist.");
        }

        var projectFiles = Directory
            .GetFiles(
                projectsDirectory,
                "*.json",
                SearchOption.TopDirectoryOnly)
            .OrderBy(
                Path.GetFileName,
                StringComparer.Ordinal)
            .ToArray();

        var duplicateFile = projectFiles
            .GroupBy(
                Path.GetFileName,
                StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateFile is not null)
        {
            throw new ContentValidationException(
                "LOAD-DUPLICATE-FILE",
                projectsDirectory,
                $"Duplicate project source filename '{duplicateFile.Key}'.");
        }

        var profile = Read<ApprovedProfile>(profilePath);
        var facets = Read<IReadOnlyList<FacetDefinition>>(facetsPath);
        var technologies =
            Read<IReadOnlyList<TechnologyDefinition>>(
                technologiesPath);

        var projects = projectFiles
            .Select(Read<ApprovedProject>)
            .OrderBy(project => project.Order)
            .ToArray();

        return new ApprovedPortfolio(
            profile,
            facets,
            technologies,
            projects);
    }

    private static string RequiredFile(
        string directory,
        string filename)
    {
        var path = Path.Combine(directory, filename);

        if (!File.Exists(path))
        {
            throw new ContentValidationException(
                "LOAD-FILE",
                path,
                "Required source file does not exist.");
        }

        return path;
    }

    private static T Read<T>(string path)
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(
                File.ReadAllBytes(path),
                Options);

            return result
                ?? throw new ContentValidationException(
                    "LOAD-NULL",
                    path,
                    "Required document is null.");
        }
        catch (ContentValidationException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new ContentValidationException(
                "LOAD-JSON",
                path,
                exception.Message);
        }
        catch (IOException exception)
        {
            throw new ContentValidationException(
                "LOAD-IO",
                path,
                exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new ContentValidationException(
                "LOAD-IO",
                path,
                exception.Message);
        }
    }
}
