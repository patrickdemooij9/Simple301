using Newtonsoft.Json;

namespace SimpleRedirects.Core.Models
{
    public class AddRedirectResponse : BaseResponse
    {
        [JsonProperty("newRedirect")]
        public Redirect NewRedirect { get; set; }
    }
}