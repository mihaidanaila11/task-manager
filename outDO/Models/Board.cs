
using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class Board
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ProjectId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual Project Project { get; set; }


    }
}
