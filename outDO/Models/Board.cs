
using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class Board
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string ProjectId { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual Project? Project { get; set; }


    }
}
