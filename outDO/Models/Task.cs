using System.ComponentModel.DataAnnotations;

namespace outDO.Models
{
    public class Task : IValidatableObject
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string BoardId { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public string Status { get; set; } = "Not Started";

        public DateTime? DateStart { get; set; }

        public DateTime? DateFinish { get; set; }

        //poze
        public string? Media {  get; set; }

        // Suport doar pentru clipuri pe youtube
        public string? Video { get; set; }

        public virtual Board? Board { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateStart > DateFinish)
            {
                yield return new ValidationResult("The start date has to be before the finish date");
            }

            if(Video != null)
            {
                Uri videoUri = null;

                try
                {
                    videoUri = new Uri(Video);
                }
                catch (Exception e) { }

                if (videoUri == null)
                {
                    yield return new ValidationResult("Not a valid link");
                }
                else
                {
                    string[] allowedHosts = {
                    "www.youtube.com",
                    "youtube.com",
                    "youtu.be",
                    "www.tiktok.com"
                };

                    if (!allowedHosts.Contains(videoUri.Host))
                    {
                        yield return new ValidationResult("Video link is not supported - Only YouTube links");
                    }
                }

                
            }
        }
    }
}
