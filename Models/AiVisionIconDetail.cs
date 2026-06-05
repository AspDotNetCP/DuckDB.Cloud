namespace DuckDB.Cloud.Models;

/// <summary>
/// Represents stored icon analysis results from an AI Vision provider.
/// </summary>
public class AiVisionIconDetail
{
    public int Id { get; set; }

    /// <summary>User who performed the icon scan (<see cref="User.Id"/>).</summary>
    public int? UserId { get; set; }

    /// <summary>Repeat scans for the same user + app_name (1–3, see upsert_ai_vision_scan.sql).</summary>
    public int ScanCount { get; set; } = 1;

    public string Provider { get; set; } = string.Empty;
    public string? OriginalPrompt { get; set; }
    public string? RawResponse { get; set; }
    public bool Success { get; set; }
    public int? ElapsedMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AppName { get; set; }
    public string? Publisher { get; set; }
    public string? Category { get; set; }
    public int? Confidence { get; set; }
    public string? Url { get; set; }
    public string? DownloadUrl { get; set; }
    public string? SourceImageHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
