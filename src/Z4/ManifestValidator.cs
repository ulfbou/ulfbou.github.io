using System.Security.Cryptography;
using System.Text;

namespace Saos.Z4;

/// <summary>
/// Manifest Validator — Ensures manifest integrity.
/// Validates SHA256 manifest_hash against published artifact.
/// Decision #1: Hash is SHA-256 hex lowercase.
/// </summary>
public static class ManifestValidator
{
    /// <summary>
    /// Validates the manifest hash against the computed hash of the artifact.
    /// </summary>
    public static bool Validate(string manifestHash, string artifactContent)
    {
        // Compute SHA256 hash of the artifact content
        using var sha256 = SHA256.Create();
        byte[] artifactBytes = Encoding.UTF8.GetBytes(artifactContent);
        byte[] computedHashBytes = sha256.ComputeHash(artifactBytes);
        string computedHash = BitConverter.ToString(computedHashBytes).Replace("-", "").ToLowerInvariant();

        // Compare computed hash with manifest hash
        return computedHash == manifestHash;
    }
}
