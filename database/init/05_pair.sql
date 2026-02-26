CREATE TABLE "Pair" (
    pair_id  SERIAL PRIMARY KEY,
    deck_id  INT NOT NULL REFERENCES "Deck"(deck_id) ON DELETE CASCADE,
    item1_id INT NOT NULL REFERENCES "Item"(item_id) ON DELETE CASCADE,
    item2_id INT NOT NULL REFERENCES "Item"(item_id) ON DELETE CASCADE,
    position INT NOT NULL DEFAULT 0,
    CONSTRAINT different_items CHECK (item1_id <> item2_id)
);
