namespace DuckDB.Cloud.Interfaces;

/// <summary>
/// Interface for managing DuckDB cloud connections with multiple database support
/// </summary>
public interface IDuckDbConnectionManager
{
    /// <summary>
    /// Get a connection to a specific database
    /// </summary>
    /// <param name="connectionName">The name of the connection (e.g., "Development", "Production")</param>
    /// <returns>A connection object</returns>
    Task<dynamic> GetConnectionAsync(string connectionName);

    /// <summary>
    /// Get all configured connection names
    /// </summary>
    IEnumerable<string> GetAvailableConnections();

    /// <summary>
    /// Execute SQL query on a specific connection
    /// </summary>
    Task<object?> ExecuteQueryAsync(string connectionName, string sql);

    /// <summary>
    /// Execute SQL command (non-query) on a specific connection
    /// </summary>
    Task ExecuteCommandAsync(string connectionName, string sql);

    /// <summary>
    /// Test connection to verify it works
    /// </summary>
    Task<bool> TestConnectionAsync(string connectionName);
}
