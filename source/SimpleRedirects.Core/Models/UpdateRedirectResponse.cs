using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SimpleRedirects.Core.Models
{
    public class UpdateRedirectResponse
    {
        [JsonProperty("updatedRedirect")]
        public Redirect UpdatedRedirect { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}