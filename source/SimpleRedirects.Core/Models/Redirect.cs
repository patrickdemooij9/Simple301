using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SimpleRedirects.Core.Models
{
    [TableName("Redirects")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    public class Redirect
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Column("IsRegex")]
        [JsonProperty("isRegex")]
        public bool IsRegex { get; set; }

        [Column("OldUrl")]
        [JsonProperty("oldUrl")]
        public string OldUrl { get; set; }

        [Column("NewUrl")]
        [JsonProperty("newUrl")]
        public string NewUrl { get; set; }

        [Column("RedirectCode")]
        [JsonProperty("redirectCode")]
        public int RedirectCode { get; set; }

        [Column("LastUpdated")]
        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [Column("Notes")]
        [JsonProperty("notes")]
        public string Notes { get; set; }

        public string GetNewUrl(Uri uri)
        {
            if (!IsRegex || !NewUrl.Contains($"$"))
                return NewUrl;

            try
            {
                var regexNewUrl = NewUrl;
                var match = Regex.Match(uri.AbsoluteUri, OldUrl);

                for (var i = 1; i < match.Groups.Count; i++)
                {
                    regexNewUrl = regexNewUrl.Replace($"${i}", match.Groups[i].Value);
                }

                return regexNewUrl;
            }
            catch (Exception)
            {
                return NewUrl;
            }
        }
    }
}