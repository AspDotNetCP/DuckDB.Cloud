namespace DuckDB.Cloud.Models;

/// <summary>
/// Configuration model for DuckDB cloud connections
/// </summary>
public class DuckDbConnectionConfig
{
    public required string Name { get; set; }
    public required string ConnectionString { get; set; }
    public string? MotherDuckToken { get; set; }
    public int CommandTimeout { get; set; } = 30;
    public bool IsReadOnly { get; set; } = false;
    public required string Database { get; set; }
}

/// <summary>
/// Collection of connection configurations
/// </summary>
public class DuckDbConnectionSettings
{
    public List<DuckDbConnectionConfig> Connections { get; set; } = new();
    public string? DefaultConnection { get; set; }
}
