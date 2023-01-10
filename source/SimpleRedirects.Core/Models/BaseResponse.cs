using Newtonsoft.Json;

namespace SimpleRedirects.Core.Models;

public abstract class BaseResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }
}