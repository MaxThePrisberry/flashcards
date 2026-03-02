using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flashcards.APIs.Entities
{
    [Table("Type")]
    public class CardType
    {
        public const string TextTypeName = "text";
        public static readonly Guid TextTypeId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        [Key]
        [Column("type_id")]
        public Guid TypeId { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("type_name")]
        public string TypeName { get; set; } = null!;
    }
}
