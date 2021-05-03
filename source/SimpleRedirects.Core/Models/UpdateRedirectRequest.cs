using System.ComponentModel.DataAnnotations;

namespace SimpleRedirects.Core.Models
{
    public class UpdateRedirectRequest
    {
        [Required]
        public Redirect Redirect { get; set; }
    }
}