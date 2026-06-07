-- Update: add scanner user to existing AiVisionIconDetails deployments
ALTER TABLE AiVisionIconDetails ADD COLUMN IF NOT EXISTS user_id INTEGER NOT NULL DEFAULT 1;

CREATE INDEX IF NOT EXISTS idx_ai_vision_icon_details_user_id ON AiVisionIconDetails(user_id);
