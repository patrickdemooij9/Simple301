
using System.ComponentModel.DataAnnotations;

namespace SimpleRedirects.Core.Models
{
    public class AddRedirectRequest
    {
        public bool IsRegex { get; set; }

        [Required]
        public string OldUrl { get; set; }
        [Required]
        public string NewUrl { get; set; }

        [Required]
        public int RedirectCode { get; set; }

        public string Notes { get; set; }
    }
}
