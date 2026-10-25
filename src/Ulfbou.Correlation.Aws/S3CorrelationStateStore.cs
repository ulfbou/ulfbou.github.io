using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Ulfbou.Correlation.Application;
using Ulfbou.Correlation.Core;

namespace Ulfbou.Correlation.Aws;

public sealed class S3CorrelationStateStore(IAmazonS3 s3, string bucketName) : ICorrelationStateStore
{
    public async Task CreateAsync(ActiveCorrelation state, CancellationToken cancellationToken)
    {
        var bytes = ActiveStateCodec.Encode(state);
        using var body = new MemoryStream(bytes, writable: false);
        try
        {
            await s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucketName,
                Key = DeletionAuthorizer.DeriveKey(state.CorrelationId),
                InputStream = body,
                ContentType = "application/json",
                IfNoneMatch = "*",
                AutoCloseStream = false
            }, cancellationToken);
        }
        catch (AmazonS3Exception error) when (error.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            throw new CorrelationFailure("STORE-EXISTS", "Correlation already exists.");
        }
    }

    public async Task<ActiveCorrelation?> ReadAsync(string correlationId, CancellationToken cancellationToken)
    {
        var observed = await ReadObservedAsync(correlationId, cancellationToken);
        return observed?.State;
    }

    public async Task<ActiveCorrelation> AdvanceAsync(ActiveCorrelation expected, ActiveCorrelation next, CancellationToken cancellationToken)
    {
        ValidateChain(expected, next);
        var observed = await ReadObservedAsync(expected.CorrelationId, cancellationToken)
            ?? throw new CorrelationFailure("STORE-NOT-FOUND", "Active state is absent.");
        if (observed.State.StateRevision != expected.StateRevision || observed.State.StateSha256 != expected.StateSha256)
            throw new CorrelationFailure("STORE-CONFLICT", "Expected state identity is stale.");
        var bytes = ActiveStateCodec.Encode(next);
        using var body = new MemoryStream(bytes, writable: false);
        try
        {
            await s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucketName,
                Key = DeletionAuthorizer.DeriveKey(expected.CorrelationId),
                InputStream = body,
                ContentType = "application/json",
                IfMatch = observed.ETag,
                AutoCloseStream = false
            }, cancellationToken);
            return next;
        }
        catch (AmazonS3Exception error) when (error.StatusCode is HttpStatusCode.PreconditionFailed or HttpStatusCode.NotFound)
        {
            throw new CorrelationFailure("STORE-CONFLICT", "S3 conditional advance rejected stale state.");
        }
    }

    public async Task<ConditionalDeletionResult> DeleteExactAsync(DeletionRequest request, CancellationToken cancellationToken)
    {
        var observed = await ReadObservedAsync(request.CorrelationId, cancellationToken);
        if (observed is null) return new(DeletionDecision.AlreadyAbsent, null, null);
        var decision = DeletionAuthorizer.Authorize(request, new(
            ActiveStatePresence.Present, observed.State.CorrelationId, observed.State.StateRevision,
            observed.State.StateSha256, "correlation-state-canonical-json-v1", observed.VersionId));
        if (decision != DeletionDecision.DeleteConditionally) return new(decision, observed.VersionId, observed.ETag);
        try
        {
            await s3.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = DeletionAuthorizer.DeriveKey(request.CorrelationId),
                VersionId = observed.VersionId,
                IfMatch = observed.ETag
            }, cancellationToken);
            return new(DeletionDecision.DeleteConditionally, observed.VersionId, observed.ETag);
        }
        catch (AmazonS3Exception error) when (error.StatusCode is HttpStatusCode.PreconditionFailed or HttpStatusCode.NotFound)
        {
            return new(DeletionDecision.RevisionConflict, observed.VersionId, observed.ETag);
        }
    }

    private async Task<ObservedObject?> ReadObservedAsync(string correlationId, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await s3.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucketName,
                Key = DeletionAuthorizer.DeriveKey(correlationId)
            }, cancellationToken);
            using var buffer = new MemoryStream();
            await response.ResponseStream.CopyToAsync(buffer, cancellationToken);
            if (buffer.Length > ActiveStateCodec.MaximumBytes) throw new CorrelationFailure("STORE-OVERSIZED", "Active state exceeds 16 KiB.");
            return new(ActiveStateCodec.Decode(buffer.ToArray()), response.ETag, response.VersionId);
        }
        catch (AmazonS3Exception error) when (error.StatusCode == HttpStatusCode.NotFound) { return null; }
    }

    private static void ValidateChain(ActiveCorrelation expected, ActiveCorrelation next)
    {
        if (next.CorrelationId != expected.CorrelationId || next.OperationId != expected.OperationId ||
            next.StateRevision != expected.StateRevision + 1 || next.PreviousStateSha256 != expected.StateSha256)
            throw new CorrelationFailure("STORE-ADVANCE", "Next state is not an exact revision chain.");
    }
    private sealed record ObservedObject(ActiveCorrelation State, string ETag, string VersionId);
}

public sealed record ConditionalDeletionResult(DeletionDecision Decision, string? ObservedVersionId, string? ObservedETag);
