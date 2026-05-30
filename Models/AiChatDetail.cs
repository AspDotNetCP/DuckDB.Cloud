namespace DuckDB.Cloud.Models;

/// <summary>
/// Represents stored chat response results from an AI Chat provider.
/// </summary>
public class AiChatDetail
{
    public int Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? OriginalPrompt { get; set; }
    public string? RawResponse { get; set; }
    public bool Success { get; set; }
    public int? ElapsedMs { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsRateLimit { get; set; }
    public bool IsQuota { get; set; }
    public bool IsUnavailable { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
