
using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class Board
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Proiectul este obligatoriu")]
        public int? ProjectId { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu")]
        [MinLength(3, ErrorMessage = "Numele trebuie sa aiba minim 3 caractere")]
        public string Name { get; set; }

        [Required]
        public virtual Project? Project { get; set; }


    }
}
