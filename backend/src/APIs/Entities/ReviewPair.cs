using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flashcards.APIs.Entities
{
    [Table("ReviewPair")]
    public class ReviewPair
    {
        [Key]
        [Column("review_id")]
        public Guid ReviewPairId { get; set; }

        [Column("history_id")]
        public Guid HistoryId { get; set; }

        [Column("pair_id")]
        public Guid PairId { get; set; }

        [ForeignKey("HistoryId")]
        public ReviewHistory ReviewHistory { get; set; } = null!;

        [ForeignKey("PairId")]
        public Pair Pair { get; set; } = null!;
    }
}
