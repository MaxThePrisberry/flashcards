using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flashcards.APIs.Entities
{
    [Table("Pair")]
    public class Pair
    {
        [Key]
        [Column("pair_id")]
        public int PairId { get; set; }

        [Column("deck_id")]
        public int DeckId { get; set; }

        [Column("item1_id")]
        public int Item1Id { get; set; }

        [Column("item2_id")]
        public int Item2Id { get; set; }

        [Column("position")]
        public int Position { get; set; } = 0;

        [ForeignKey("DeckId")]
        public Deck Deck { get; set; } = null!;

        [ForeignKey("Item1Id")]
        public Item Item1 { get; set; } = null!;

        [ForeignKey("Item2Id")]
        public Item Item2 { get; set; } = null!;
    }
}
