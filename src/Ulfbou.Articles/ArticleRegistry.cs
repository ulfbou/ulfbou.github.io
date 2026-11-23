using System;
using System.Collections.Generic;
using System.Text;

namespace Ulfbou.Articles;

public sealed record ArticleRegistry(
    string SchemaVersion,
    Article[] Articles);

public sealed record SeriesRegistry(
    string SchemaVersion,
    Series[] Series);

public sealed record Series(
    string Id,
    string Title,
    string Description,
    string Status);

public sealed record Approval(
    bool Approved,
    string? ApprovedBy,
    string? ApprovedDate);

public sealed record Article(
    string Id,
    string Slug,
    string Status,
    string Title,
    string Deck,
    string Description,
    string Author,
    string Language,
    string? PublishedDate,
    string? ModifiedDate,
    int ReadingMinutes,
    string BodyPath,
    string SeriesId,
    int SeriesPart,
    string SocialImage,
    string SocialImageAlt,
    string CanonicalPath,
    Approval EditorialApproval);

public sealed record PublicationProjection(
    string SchemaVersion,
    PublishedArticle[] Articles);

public sealed record PublishedArticle(
    string SourceId,
    string SourcePath,
    string BodySourcePath,
    string Status,
    string Slug,
    string CanonicalPath,
    string OutputPath,
    string Title,
    string Description,
    string PublishedDate,
    string ModifiedDate,
    string SeriesId,
    int SeriesPart,
    string ContentSha256);
