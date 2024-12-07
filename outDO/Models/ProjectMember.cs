using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace outDO.Models
{
    public class ProjectMember
    {

        public int UserId { get; set; }
        public int ProjectId { get; set; }
        [Required]
        public string ProjectRole { get; set; }

        public virtual User User { get; set; }
        public virtual Project Project { get; set; }

    }
}
