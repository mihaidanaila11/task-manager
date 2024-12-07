using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int TaskId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public DateTime Date { get; set; }

        public virtual Task Task { get; set; }
        public virtual User User { get; set; }
    }
}
