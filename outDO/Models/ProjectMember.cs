using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace outDO.Models
{
    public class ProjectMember
    {
        [Key]
        public int Id { get; set; }
        //pasull 6 user si roluri
        //cheia externa - many to many user si proiecte
        public string? UserId { get; set; }
        public int? ProjectId { get; set; }
      
        public string? ProjectRole { get; set; }

        //proprietate virtuala
        public virtual User? User { get; set; }
        public virtual Project? Project { get; set; }

    }
}
