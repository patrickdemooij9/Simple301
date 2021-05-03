using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SimpleRedirects.Core.Models
{
    public class DeleteRedirectResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}