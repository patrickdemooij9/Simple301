using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SimpleRedirects.Core.Models
{
    public class AddRedirectResponse
    {
        [JsonProperty("newRedirect")]
        public Redirect NewRedirect { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}