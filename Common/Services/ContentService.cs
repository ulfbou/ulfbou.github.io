﻿using System.Net.Http.Json;
using System.Text.Json;

using Ulfbou.GitHub.IO.Common.Models;

namespace Ulfbou.GitHub.IO.Common.Services;

public class ContentService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    public const string BaseUri = "content";

    private Metadata? _metadata;
    private List<string>? _tags;
    private List<string>? _categories;
    private List<string>? _keywords;

    public async Task<Metadata> GetMetadata(string? section = null)
    {
        if (_metadata == null)
        {
            try
            {
                var path = string.IsNullOrWhiteSpace(section) ? $"{BaseUri}/metadata.json" : $"{BaseUri}/{section}/metadata.json";
                var response = await _httpClient.GetAsync($"{BaseUri}/metadata.json");
                response.EnsureSuccessStatusCode();
                _metadata = await JsonSerializer.DeserializeAsync<Metadata>(await response.Content.ReadAsStreamAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            Console.WriteLine("Metadata loaded: {0}", _metadata?.Posts.Count().ToString() ?? "null");
            _metadata ??= new();
        }

        return _metadata!;
    }

    public async Task<TData?> GetJson<TData>(string? section = null) where TData : class
    {
        try
        {
            var path = string.IsNullOrWhiteSpace(section) ? $"{BaseUri}/metadata.json" : $"{BaseUri}/{section}/metadata.json";
            var response = await _httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            return await JsonSerializer.DeserializeAsync<TData>(await response.Content.ReadAsStreamAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public async Task<IEnumerable<string>> GetCategories()
    {
        if (_categories == null)
        {
            var metadata = await GetMetadata();
            _categories = metadata.Posts.SelectMany(x => x.Categories).Distinct().ToList();
        }
        Console.WriteLine("Categories loaded: {0}", _categories.Count());
        return _categories;
    }

    public async Task<IEnumerable<string>> GetTags()
    {
        if (_tags == null)
        {
            var metadata = await GetMetadata();
            _tags = metadata.Posts.SelectMany(item => item.Tags).Distinct().ToList();
        }

        Console.WriteLine("Tags loaded: {0}", _tags.Count());
        return _tags;
    }

    public async Task<IEnumerable<string>> GetCategoryTags(string category)
    {
        var tags = await GetTags();
        return tags.Where(x => x.StartsWith(category));
    }

    public async Task<IEnumerable<string>> GetKeywords()
    {
        if (_keywords == null)
        {
            var metadata = await GetMetadata();
            _keywords = metadata.Posts.SelectMany(x => x.Keywords).Distinct().ToList();
        }

        return _keywords;
    }

    public async Task<IEnumerable<string>> GetCategoryKeywords(string category)
    {
        var keywords = await GetKeywords();
        return keywords.Where(x => x.StartsWith(category));
    }

    public async Task<IEnumerable<PostMetadata>> GetContentByCategory(string category)
    {
        var metadata = await GetMetadata();
        return metadata.Posts.Where(post => post.Categories.Contains(category));
    }

    public async Task<IEnumerable<PostMetadata>> GetContentByTagAsync(string tag)
    {
        var metadata = await GetMetadata();
        return metadata.Posts.Where(post => post.Tags.Contains(tag));
    }

    public async Task<IEnumerable<PostMetadata>> GetRelatedContent(PostMetadata currentItem)
    {
        var metadata = await GetMetadata();
        return metadata.Posts.Where(post => post != currentItem && post.Tags.Any(tag => currentItem.Tags.Contains(tag)));
    }
}