namespace DuckDB.Cloud.Models;

/// <summary>
/// Developer / company info stored when a scan has no public market data
/// (Panel 3 "Developer info" path). Scans with market data go to stock_data.
/// </summary>
[DuckDbTable("DeveloperInfo")]
public class DeveloperInfo
{
    public int Id { get; set; }
    public int UserId { get; set; } = 1;
    public int? AiVisionIconDetailId { get; set; }
    public string? AppName { get; set; }
    public string? Company { get; set; }
    public string? Website { get; set; }
    public string? DownloadUrl { get; set; }
    // NOTE: named without interior capitals so GenericRepo's ToSnakeCase
    // yields "github"/"linkedin" to match the real column names.
    public string? Github { get; set; }
    public string? Linkedin { get; set; }
    public string? Twitter { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public string? RawInfoText { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}