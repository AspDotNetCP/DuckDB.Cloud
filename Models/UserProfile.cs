namespace DuckDB.Cloud.Models;

/// <summary>
/// Represents a user profile linked to a user in the main `users` table.
/// </summary>
public class UserProfile
{
    public int Id { get; set; }

    // Foreign key to `users.id`
    public int UserId { get; set; }
    
    // Navigation property (optional; useful for object mapping)
    public User? User { get; set; }

    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }

    // JSON or serialized preferences blob (stored as TEXT in DuckDB)
    public string? Preferences { get; set; }

    public bool IsPublic { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
