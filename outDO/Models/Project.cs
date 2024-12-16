using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class Project
    {

        [Key]
        public string Id { get; set; }
        [Required(ErrorMessage ="Name Required")]
        [MinLength(4, ErrorMessage ="Name has to be at least 4 characters long")]
        public string Name { get; set; }
        
        public string? Background { get; set; }

        //un proiect are mai multe board-uri
        public virtual ICollection<Board>? Boards { get; set; }
        public virtual ICollection<ProjectMember>? ProjectMembers { get; set; }
    }
}
