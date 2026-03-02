CREATE TABLE "Type" (
    type_id    UUID PRIMARY KEY,
    type_name  VARCHAR(10) NOT NULL UNIQUE  -- 'text' or 'image'
);

-- Seed the two valid types immediately
INSERT INTO "Type" (type_id, type_name) VALUES
    ('11111111-1111-1111-1111-111111111111', 'text'),
    ('22222222-2222-2222-2222-222222222222', 'image');
