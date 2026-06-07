-- Migration: 003_ai_vision_icon_details.sql
-- Stores AI Vision icon analysis results for the My Icon workflow

CREATE SEQUENCE IF NOT EXISTS seq_ai_vision_icon_details_id START 1;

CREATE TABLE IF NOT EXISTS AiVisionIconDetails (
    id INTEGER PRIMARY KEY DEFAULT nextval('seq_ai_vision_icon_details_id'),
    user_id INTEGER NOT NULL DEFAULT 1,
    scan_count INTEGER NOT NULL DEFAULT 1,
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
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CHECK (scan_count >= 1 AND scan_count <= 3)
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_ai_vision_icon_details_user_app
    ON AiVisionIconDetails(user_id, app_name);

CREATE INDEX IF NOT EXISTS idx_ai_vision_icon_details_user_id ON AiVisionIconDetails(user_id);
CREATE INDEX IF NOT EXISTS idx_ai_vision_icon_details_provider ON AiVisionIconDetails(provider);
CREATE INDEX IF NOT EXISTS idx_ai_vision_icon_details_app_name ON AiVisionIconDetails(app_name);
