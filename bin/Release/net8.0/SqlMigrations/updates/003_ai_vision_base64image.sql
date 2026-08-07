-- Update: add base64image column, drop source_image_hash, clean up stale v2 table
ALTER TABLE AiVisionIconDetails ADD COLUMN IF NOT EXISTS base64image TEXT;
ALTER TABLE AiVisionIconDetails DROP COLUMN IF EXISTS source_image_hash;
DROP TABLE IF EXISTS AiVisionIconDetails_v2;
