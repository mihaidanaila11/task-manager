using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string role { get; set; }
        public string profilePicture { get; set; }

        public virtual ICollection<ProjectMember> ProjectMembers { get; set; }

    }
}
