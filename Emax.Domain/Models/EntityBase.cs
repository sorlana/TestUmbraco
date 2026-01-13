using System.ComponentModel.DataAnnotations;

namespace Emax.Domain.Models
{
    public class EntityBase
    {
        protected EntityBase() => DateAdded = DateTime.UtcNow;

        [DataType(DataType.Time)]
        public DateTime DateAdded { get; set; }


        [Required]
        [Key]
        public Guid Id { get; set; }

    }
}
