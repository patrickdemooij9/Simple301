using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SimpleRedirects.Core.Models
{
    public class UpdateRedirectResponse : BaseResponse
    {
        [JsonProperty("updatedRedirect")]
        public Redirect UpdatedRedirect { get; set; }
    }
}