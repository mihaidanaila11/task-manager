using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class Comment
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string? TaskId { get; set; }
        [Required]
        public string? UserId { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public DateTime Date { get; set; }

        public virtual Task? Task { get; set; }
        public virtual User?  User { get; set; }
    }
}
