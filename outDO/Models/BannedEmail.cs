using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class BannedEmail
    {
        [Key]
        public string email {  get; set; }
    }
}
