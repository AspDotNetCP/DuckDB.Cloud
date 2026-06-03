namespace DuckDB.Cloud.Schema;

/// <summary>
/// CREATE TABLE DDL for entities persisted via GenericRepo.
/// Uses sequences (MotherDuck does not implement IDENTITY constraints).
/// </summary>
public static class DuckDbEntitySchemas
{
    private static readonly IReadOnlyDictionary<Type, string> CreateTableSqlByType =
        new Dictionary<Type, string>
        {
            [typeof(Models.AiVisionIconDetail)] = """
                CREATE SEQUENCE IF NOT EXISTS seq_ai_vision_icon_details_id START 1;
                CREATE TABLE IF NOT EXISTS AiVisionIconDetails (
                    id INTEGER PRIMARY KEY DEFAULT nextval('seq_ai_vision_icon_details_id'),
                    provider VARCHAR NOT NULL,
                    original_prompt TEXT,
                    raw_response TEXT,
                    success BOOLEAN NOT NULL DEFAULT FALSE,
                    elapsed_ms INTEGER,
                    error_message TEXT,
                    app_name VARCHAR,
                    publisher VARCHAR,
                    category VARCHAR,
                    confidence INTEGER,
                    url VARCHAR,
                    download_url VARCHAR,
                    source_image_hash VARCHAR,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
                CREATE INDEX IF NOT EXISTS idx_ai_vision_icon_details_provider ON AiVisionIconDetails(provider);
                CREATE INDEX IF NOT EXISTS idx_ai_vision_icon_details_app_name ON AiVisionIconDetails(app_name);
                """,
            [typeof(Models.AiChatDetail)] = """
                CREATE SEQUENCE IF NOT EXISTS seq_ai_chat_details_id START 1;
                CREATE TABLE IF NOT EXISTS AiChatDetails (
                    id INTEGER PRIMARY KEY DEFAULT nextval('seq_ai_chat_details_id'),
                    provider VARCHAR NOT NULL,
                    original_prompt TEXT,
                    raw_response TEXT,
                    success BOOLEAN NOT NULL DEFAULT FALSE,
                    elapsed_ms INTEGER,
                    error_message TEXT,
                    is_rate_limit BOOLEAN NOT NULL DEFAULT FALSE,
                    is_quota BOOLEAN NOT NULL DEFAULT FALSE,
                    is_unavailable BOOLEAN NOT NULL DEFAULT FALSE,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
                """
        };

    public static bool TryGetCreateTableSql(Type entityType, out string sql) =>
        CreateTableSqlByType.TryGetValue(entityType, out sql!);
}
