using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        
        public string? Background { get; set; }

        //un proiect are mai multe board-uri
        public virtual ICollection<Board>? Boards { get; set; }
        public virtual ICollection<ProjectMember> ProjectMembers { get; set; }
    }
}
