using Microsoft.Extensions.Logging;
using DuckDB.Cloud.Models;
using DuckDB.Cloud.Interfaces;

namespace DuckDB.Cloud;

/// <summary>
/// Main DuckDB context for managing cloud database operations
/// </summary>
public class DuckDbContext
{
    private readonly DuckDbConnectionSettings _settings;
    private readonly ILogger<DuckDbContext> _logger;
    private readonly IDuckDbConnectionManager _connectionManager;

    public DuckDbContext(DuckDbConnectionSettings settings, ILogger<DuckDbContext> logger, IDuckDbConnectionManager connectionManager)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
    }

    /// <summary>
    /// Initialize DuckDB context - loads configuration and validates connections
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInformation($"Initializing DuckDB context with {_settings.Connections.Count} connection(s)");

        foreach (var connection in _settings.Connections)
        {
            _logger.LogInformation($"Registered connection: {connection.Name} -> {connection.Database}");
        }

        if (string.IsNullOrEmpty(_settings.DefaultConnection))
        {
            _settings.DefaultConnection = _settings.Connections.FirstOrDefault()?.Name ?? "default";
            _logger.LogWarning($"No default connection specified. Using: {_settings.DefaultConnection}");
        }

        // Run migrations
        await RunMigrationsAsync();
    }

    /// <summary>
    /// Run SQL migrations to create/update database schema
    /// </summary>
    private async Task RunMigrationsAsync()
    {
        try
        {
            var connectionName = _settings.DefaultConnection ?? "Production";
            _logger.LogInformation("Running DuckDB migrations on connection: {Connection}", connectionName);

            // Migration 003: Create AiVisionIconDetails table
            var migrationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "DuckDB.Cloud", "SqlMigrations", "003_ai_vision_icon_details.sql");
            
            if (File.Exists(migrationPath))
            {
                var sql = await File.ReadAllTextAsync(migrationPath);
                await _connectionManager.ExecuteCommandAsync(connectionName, sql);
                _logger.LogInformation("Migration 003_ai_vision_icon_details.sql executed successfully");
            }
            else
            {
                _logger.LogWarning("Migration file not found: {Path}", migrationPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run DuckDB migrations");
            // Don't throw - allow app to start even if migrations fail
        }
    }

    /// <summary>
    /// Get connection settings
    /// </summary>
    public DuckDbConnectionSettings GetSettings() => _settings;

    /// <summary>
    /// Get configuration for a specific connection
    /// </summary>
    public DuckDbConnectionConfig? GetConnectionConfig(string connectionName)
    {
        return _settings.Connections.FirstOrDefault(c => 
            c.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// List all available connections
    /// </summary>
    public IEnumerable<string> GetConnectionNames() => _settings.Connections.Select(c => c.Name);
}
