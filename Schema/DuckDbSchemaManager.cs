using System.Data.Common;
using System.Reflection;
using DuckDB.Cloud.Interfaces;
using DuckDB.NET.Data;
using Microsoft.Extensions.Logging;

namespace DuckDB.Cloud.Schema;

public sealed class DuckDbSchemaManager : IDuckDbSchemaManager
{
    private static readonly SemaphoreSlim SchemaGate = new(1, 1);
    private static volatile bool _schemaEnsured;

    private readonly IDuckDbConnectionManager _connectionManager;
    private readonly ILogger<DuckDbSchemaManager> _logger;

    public DuckDbSchemaManager(
        IDuckDbConnectionManager connectionManager,
        ILogger<DuckDbSchemaManager> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task EnsureSchemaAsync(
        string connectionName = "Production",
        CancellationToken cancellationToken = default)
    {
        if (_schemaEnsured)
            return;

        await SchemaGate.WaitAsync(cancellationToken);
        try
        {
            if (_schemaEnsured)
                return;

            await EnsureMigrationHistoryTableAsync(connectionName, cancellationToken);
            await EnsureCreateTablesAsync(connectionName, cancellationToken);
            await EnsureTableUpdatesAsync(connectionName, cancellationToken);
            _schemaEnsured = true;
            _logger.LogInformation("DuckDB schema ensured on connection {Connection}", connectionName);
        }
        finally
        {
            SchemaGate.Release();
        }
    }

    public async Task EnsureCreateTablesAsync(
        string connectionName = "Production",
        CancellationToken cancellationToken = default)
    {
        var migrationsDir = ResolveMigrationsDirectory();
        if (migrationsDir == null)
        {
            _logger.LogWarning("SqlMigrations directory not found; skipping EnsureCreateTables");
            return;
        }

        var appliedScripts = await GetAppliedScriptsAsync(connectionName, cancellationToken);

        foreach (var file in Directory.GetFiles(migrationsDir, "*.sql")
                     .Select(Path.GetFileName)
                     .Where(f => f != null && IsCreateMigration(f!))
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var path = Path.Combine(migrationsDir, file!);
            var tableName = ExtractTableNameFromMigration(file!);
            
            // Check if migration was already applied
            if (appliedScripts.Contains(file!))
            {
                // Migration was applied, but check if table still exists
                // If table was deleted manually, re-run the migration
                if (!string.IsNullOrEmpty(tableName) && await TableExistsAsync(connectionName, tableName, cancellationToken))
                {
                    _logger.LogDebug("Migration {Script} already applied and table {Table} exists, skipping", file, tableName);
                    continue;
                }
                
                // Table was deleted, need to re-run migration
                _logger.LogInformation("Migration {Script} was applied but table {Table} was deleted, re-running", file, tableName);
                await RemoveMigrationRecordAsync(connectionName, file!, cancellationToken);
            }

            await ExecuteSqlFileAsync(connectionName, path, file!, cancellationToken);
        }
    }

    private static string? ExtractTableNameFromMigration(string fileName)
    {
        // Extract table name from migration file (e.g., "006_developer_info.sql" -> "DeveloperInfo")
        var migrationName = Path.GetFileNameWithoutExtension(fileName);
        // Skip the numeric prefix (e.g., "006_")
        var underscoreIndex = migrationName.IndexOf('_');
        if (underscoreIndex >= 0 && underscoreIndex + 1 < migrationName.Length)
        {
            var name = migrationName.Substring(underscoreIndex + 1);
            // Convert snake_case to PascalCase
            return string.Join("", name.Split('_').Select(s => 
                string.IsNullOrEmpty(s) ? "" : char.ToUpper(s[0]) + s.Substring(1)));
        }
        return null;
    }

    private async Task<bool> TableExistsAsync(string connectionName, string tableName, CancellationToken cancellationToken)
    {
        try
        {
            var connection = (DuckDBConnection)await _connectionManager.GetConnectionAsync(connectionName);
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{tableName}';";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result != null && Convert.ToInt32(result) > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task RemoveMigrationRecordAsync(string connectionName, string scriptName, CancellationToken cancellationToken)
    {
        var escaped = scriptName.Replace("'", "''", StringComparison.Ordinal);
        var sql = $"DELETE FROM schema_migrations WHERE script_name = '{escaped}';";
        await _connectionManager.ExecuteCommandAsync(connectionName, sql);
    }

    public async Task EnsureTableUpdatesAsync(
        string connectionName = "Production",
        CancellationToken cancellationToken = default)
    {
        var updatesDir = ResolveUpdatesDirectory();
        if (updatesDir == null || !Directory.Exists(updatesDir))
        {
            _logger.LogDebug("SqlMigrations/updates not found; skipping EnsureTableUpdates");
            return;
        }

        var appliedScripts = await GetAppliedScriptsAsync(connectionName, cancellationToken);

        foreach (var path in Directory.GetFiles(updatesDir, "*.sql").OrderBy(p => p, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var label = Path.GetFileName(path);
            if (label != null && appliedScripts.Contains(label))
                continue;

            await ExecuteSqlFileAsync(connectionName, path, label!, cancellationToken);
        }
    }

    private async Task ExecuteSqlFileAsync(
        string connectionName,
        string path,
        string label,
        CancellationToken cancellationToken)
    {
        var sql = await File.ReadAllTextAsync(path, cancellationToken);
        if (string.IsNullOrWhiteSpace(sql))
            return;

        try
        {
            _logger.LogInformation("Applying schema script {Script}", label);
            await _connectionManager.ExecuteCommandAsync(connectionName, sql);
            await MarkMigrationAppliedAsync(connectionName, label, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schema script {Script} failed", label);
            throw;
        }
    }

    private static bool IsCreateMigration(string fileName) =>
        fileName.Length >= 4
        && char.IsDigit(fileName[0])
        && char.IsDigit(fileName[1])
        && char.IsDigit(fileName[2])
        && fileName[3] == '_';

    internal static string? ResolveMigrationsDirectory()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "SqlMigrations"),
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "SqlMigrations"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "DuckDB.Cloud", "SqlMigrations")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "DuckDB.Cloud", "SqlMigrations")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "DuckDB.Cloud", "SqlMigrations"))
        };

        foreach (var dir in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (Directory.Exists(dir))
                return dir;
        }

        return null;
    }

    private async Task EnsureMigrationHistoryTableAsync(
        string connectionName,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS schema_migrations (
                script_name VARCHAR PRIMARY KEY,
                applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            );";

        await _connectionManager.ExecuteCommandAsync(connectionName, sql);
    }

    private async Task<HashSet<string>> GetAppliedScriptsAsync(
        string connectionName,
        CancellationToken cancellationToken)
    {
        var appliedScripts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var connection = (DuckDBConnection)await _connectionManager.GetConnectionAsync(connectionName);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT script_name FROM schema_migrations;";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (!reader.IsDBNull(0))
                appliedScripts.Add(reader.GetString(0));
        }

        return appliedScripts;
    }

    private async Task MarkMigrationAppliedAsync(
        string connectionName,
        string scriptName,
        CancellationToken cancellationToken)
    {
        var escaped = scriptName.Replace("'", "''", StringComparison.Ordinal);
        var sql = $"INSERT INTO schema_migrations (script_name) VALUES ('{escaped}');";
        await _connectionManager.ExecuteCommandAsync(connectionName, sql);
    }

    internal static string? ResolveUpdatesDirectory()
    {
        var migrations = ResolveMigrationsDirectory();
        if (migrations == null)
            return null;

        var updates = Path.Combine(migrations, "updates");
        return Directory.Exists(updates) ? updates : null;
    }
}
