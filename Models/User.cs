namespace DuckDB.Cloud.Models;

/// <summary>
/// Represents a user record from the `users` table.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property (optional; useful for object mapping)
    public UserProfile? Profile { get; set; }
}
