CREATE TABLE "Distractor" (
    distractor_id UUID PRIMARY KEY,
    pair_id       UUID     NOT NULL REFERENCES "Pair"(pair_id) ON DELETE CASCADE,
    direction     SMALLINT NOT NULL,  -- 0 = fake definition, 1 = fake term
    value         TEXT     NOT NULL,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_distractor_pair_direction_value UNIQUE (pair_id, direction, value)
);
