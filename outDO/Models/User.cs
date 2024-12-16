using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
 
//pas 1 user si roluri

namespace outDO.Models
{
    public class User : IdentityUser 
    {
        
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<ProjectMember>? ProjectMembers { get; set; }

    }
}
