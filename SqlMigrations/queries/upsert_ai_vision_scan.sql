-- Upsert a vision scan: one row per (user_id, app_name).
-- First scan inserts scan_count = 1; repeats increment scan_count up to 3.
-- When scan_count is already 3, the ON CONFLICT update is skipped (no increment, no error).
--
-- Parameters (Dapper / DuckDB $name):
--   $UserId, $AppName, $Provider, $OriginalPrompt, $RawResponse, $Success,
--   $ElapsedMs, $ErrorMessage, $Publisher, $Category, $Confidence,
--   $Url, $DownloadUrl, $SourceImageHash

INSERT INTO AiVisionIconDetails (
    user_id,
    app_name,
    scan_count,
    provider,
    original_prompt,
    raw_response,
    success,
    elapsed_ms,
    error_message,
    publisher,
    category,
    confidence,
    url,
    download_url,
    source_image_hash,
    created_at,
    updated_at
) VALUES (
    $UserId,
    $AppName,
    1,
    $Provider,
    $OriginalPrompt,
    $RawResponse,
    $Success,
    $ElapsedMs,
    $ErrorMessage,
    $Publisher,
    $Category,
    $Confidence,
    $Url,
    $DownloadUrl,
    $SourceImageHash,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
)
ON CONFLICT (user_id, app_name) DO UPDATE SET
    scan_count = LEAST(3, AiVisionIconDetails.scan_count + 1),
    provider = EXCLUDED.provider,
    original_prompt = EXCLUDED.original_prompt,
    raw_response = EXCLUDED.raw_response,
    success = EXCLUDED.success,
    elapsed_ms = EXCLUDED.elapsed_ms,
    error_message = EXCLUDED.error_message,
    publisher = EXCLUDED.publisher,
    category = EXCLUDED.category,
    confidence = EXCLUDED.confidence,
    url = EXCLUDED.url,
    download_url = EXCLUDED.download_url,
    source_image_hash = EXCLUDED.source_image_hash,
    updated_at = CURRENT_TIMESTAMP
WHERE AiVisionIconDetails.scan_count < 3;
