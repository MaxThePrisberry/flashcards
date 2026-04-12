using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flashcards.APIs.Entities
{
    [Table("Distractor")]
    public class Distractor
    {
        [Key]
        [Column("distractor_id")]
        public Guid DistractorId { get; set; }

        [Column("pair_id")]
        public Guid PairId { get; set; }

        [Column("direction")]
        public int Direction { get; set; } // 0 = fake definition, 1 = fake term

        [Column("value")]
        public string Value { get; set; } = "";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PairId")]
        public Pair Pair { get; set; } = null!;
    }
}
