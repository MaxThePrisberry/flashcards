public class Pair {
    public Guid PairId { get; set; }
    public Guid Item1Id { get; set; }
    public Guid Item2Id { get; set; }
    public int Position { get; set; }

    public Item Item1 { get; set; }
    public Item Item2 { get; set; }

    public Guid DeckId { get; set; }
    public Deck Deck { get; set; }
}
