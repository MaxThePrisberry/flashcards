public class Item {
    public Guid ItemId { get; set; }
    public Guid DeckId { get; set; }
    public Guid TypeId { get; set; }
    public string Value { get; set; }
    public int Position { get; set; }

    public Deck Deck { get; set; }
    public CardType Type { get; set; }
}
