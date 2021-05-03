using System.Linq;
using SimpleRedirects.Core.Models;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SimpleRedirects.Core.Migrations
{
    public class TrimOldUrlMigration : MigrationBase
    {
        public TrimOldUrlMigration(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            if (!TableExists("Redirects")) return;

            var redirects = Database.Fetch<Redirect>(Database.SqlContext.Sql()
                .SelectAll()
                .From<Redirect>()
                .Where<Redirect>(it => !it.IsRegex));
            foreach (var redirect in redirects)
            {
                redirect.OldUrl = CleanUrl(redirect.OldUrl);
                Database.Update(redirect);
            }
        }

        private string CleanUrl(string url)
        {
            var urlParts = url.ToLowerInvariant().Split('?');
            var baseUrl = urlParts[0].TrimEnd('/');
            return urlParts.Length == 1 ? baseUrl : $"{baseUrl}?{string.Join("?", urlParts.Skip(1))}";
        }
    }
}