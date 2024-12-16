
using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class Board
    {
        [Key]
        public string Id { get; set; }
        [Required(ErrorMessage = "Proiectul este obligatoriu")]
        public string ProjectId { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu")]
        [MinLength(3, ErrorMessage = "Numele trebuie sa aiba minim 3 caractere")]
        public string Name { get; set; }

        [Required]
        public virtual Project? Project { get; set; }


    }
}
