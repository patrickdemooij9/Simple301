using System;
using System.Linq;
using Newtonsoft.Json;

namespace SimpleRedirects.Core.Models;

public class ImportRedirectsResponse : BaseResponse
{
    [JsonProperty("addedRedirects")]
    public int AddedRedirects { get; set; }

    [JsonProperty("updatedRedirects")]
    public int UpdatedRedirects { get; set; }

    [JsonProperty("existingRedirects")]
    public int ExistingRedirects { get; set; }

    [JsonProperty("errorRedirects")]
    public Redirect[] ErrorRedirects { get; set; }

    public static ImportRedirectsResponse FromImport(int addedRedirects, int updatedRedirects, int existingRedirects, Redirect[] errorRedirects)
        => new ImportRedirectsResponse
        {
            Success = !errorRedirects.Any(),
            Message = $"Redirect import completed {(!errorRedirects.Any() ? "without errors" : $"with {errorRedirects.Length} error{FormatSingularOrPlural(errorRedirects.Length)}")},{(existingRedirects > 0 ? $" ignored {existingRedirects} redirect{FormatSingularOrPlural(existingRedirects)} because they already existed," : string.Empty)} added {addedRedirects} redirect{FormatSingularOrPlural(addedRedirects)}{(updatedRedirects > 0 ? $" and updated {updatedRedirects} redirect{FormatSingularOrPlural(updatedRedirects)}" : string.Empty)}.",
            AddedRedirects = addedRedirects,
            UpdatedRedirects = updatedRedirects,
            ExistingRedirects = existingRedirects,
            ErrorRedirects = errorRedirects
        };

    public static ImportRedirectsResponse EmptyImportRecordResponse(string message = "No valid redirects could be processed.")
        => new ImportRedirectsResponse
        {
            Success = false,
            Message = message,
            AddedRedirects = 0,
            UpdatedRedirects = 0,
            ExistingRedirects = 0,
            ErrorRedirects = Array.Empty<Redirect>()
        };

    private static string FormatSingularOrPlural(int length)
        => length == 1 ? string.Empty : "s";
}