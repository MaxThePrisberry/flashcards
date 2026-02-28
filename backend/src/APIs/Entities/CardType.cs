using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flashcards.APIs.Entities
{
    [Table("Type")]
    public class CardType
    {
        [Key]
        [Column("type_id")]
        public Guid TypeId { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("type_name")]
        public string TypeName { get; set; } = null!;
    }
}
