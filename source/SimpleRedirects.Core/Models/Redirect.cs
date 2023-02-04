using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using CsvHelper.Configuration.Attributes;
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
        [Default("False")]
        public bool IsRegex { get; set; }

        [Column("OldUrl")]
        [JsonProperty("oldUrl")]
        public string OldUrl { get; set; }

        [Column("NewUrl")]
        [JsonProperty("newUrl")]
        [Default("")]
        public string NewUrl { get; set; }

        [Column("RedirectCode")]
        [JsonProperty("redirectCode")]
        [Default(301)]
        public int RedirectCode { get; set; }

        [Column("LastUpdated")]
        [JsonProperty("lastUpdated")]
        [Default("")]
        public DateTime? LastUpdated { get; set; }

        [Column("Notes")]
        [JsonProperty("notes")]
        [Default("")]
        public string Notes { get; set; }

        public string GetNewUrl(Uri uri, bool preserveQueryString)
        {
            if (!IsRegex || !NewUrl.Contains($"$"))
            {
                var url = NewUrl;
                if (!preserveQueryString ||
                    !Uri.TryCreate(NewUrl, UriKind.RelativeOrAbsolute, out var _)) return url;

                var index = url.IndexOf('?');
                var queryString = index >= 0 ? url.Substring(index) : "";
                var query = HttpUtility.ParseQueryString(queryString);
                var appendQuery = HttpUtility.ParseQueryString(uri.Query);
                foreach (var item in appendQuery.AllKeys)
                {
                    query[item] = appendQuery.Get(item);
                }

                url = $"{url.Split('?').First()}{(query.Count > 0 ? $"?{query}" : string.Empty)}";
                return url;
            }

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