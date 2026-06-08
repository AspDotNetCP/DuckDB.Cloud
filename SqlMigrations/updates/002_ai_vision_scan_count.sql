-- Update: track repeat scans of the same app per user (max 3)
ALTER TABLE AiVisionIconDetails ADD COLUMN IF NOT EXISTS scan_count INTEGER;

UPDATE AiVisionIconDetails
SET scan_count = 1
WHERE scan_count IS NULL OR scan_count < 1;

UPDATE AiVisionIconDetails
SET scan_count = 3
WHERE scan_count > 3;

-- Collapse duplicate (user_id, app_name) rows before unique index (keeps newest id)
UPDATE AiVisionIconDetails
SET scan_count = LEAST(3, grouped.cnt)
FROM (
    SELECT user_id, app_name, MAX(id) AS keep_id, COUNT(*) AS cnt
    FROM AiVisionIconDetails
    WHERE user_id IS NOT NULL AND app_name IS NOT NULL
    GROUP BY user_id, app_name
    HAVING COUNT(*) > 1
) AS grouped
WHERE AiVisionIconDetails.id = grouped.keep_id;

DELETE FROM AiVisionIconDetails
WHERE id IN (
    SELECT v.id
    FROM AiVisionIconDetails v
    INNER JOIN (
        SELECT user_id, app_name, MAX(id) AS keep_id
        FROM AiVisionIconDetails
        WHERE user_id IS NOT NULL AND app_name IS NOT NULL
        GROUP BY user_id, app_name
        HAVING COUNT(*) > 1
    ) AS grouped
        ON v.user_id = grouped.user_id
       AND v.app_name = grouped.app_name
    WHERE v.id <> grouped.keep_id
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_ai_vision_icon_details_user_app
    ON AiVisionIconDetails(user_id, app_name);

CREATE INDEX IF NOT EXISTS idx_ai_vision_icon_details_scan_count
    ON AiVisionIconDetails(scan_count);
