using System.Reflection;
using DuckDB.Cloud.Interfaces;
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

        foreach (var file in Directory.GetFiles(migrationsDir, "*.sql")
                     .Select(Path.GetFileName)
                     .Where(f => f != null && IsCreateMigration(f!))
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(migrationsDir, file!);
            await ExecuteSqlFileAsync(connectionName, path, file!, cancellationToken);
        }
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

        foreach (var path in Directory.GetFiles(updatesDir, "*.sql").OrderBy(p => p, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteSqlFileAsync(connectionName, path, Path.GetFileName(path), cancellationToken);
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

    internal static string? ResolveUpdatesDirectory()
    {
        var migrations = ResolveMigrationsDirectory();
        if (migrations == null)
            return null;

        var updates = Path.Combine(migrations, "updates");
        return Directory.Exists(updates) ? updates : null;
    }
}
