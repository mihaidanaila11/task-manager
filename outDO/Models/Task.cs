using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int BoardId { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTime? DateStart { get; set; }

        public DateTime? DateFinish { get; set; }

        //poze
        public string? Media {  get; set; }

        public virtual Board Board { get; set; }
    }
}
