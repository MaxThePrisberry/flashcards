CREATE TABLE "FakeAnswer" (
    fake_answer_id UUID PRIMARY KEY,
    pair_id        UUID NOT NULL REFERENCES "Pair"(pair_id) ON DELETE CASCADE,
    -- The incorrect option being shown
    item_id        UUID NOT NULL REFERENCES "Item"(item_id) ON DELETE CASCADE,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    -- Prevent duplicates for same pair
    CONSTRAINT unique_fake_per_pair UNIQUE (pair_id, item_id)
);
