using System;
using System.Linq;
using SimpleRedirects.Core.Models;
using Umbraco.Core.Migrations;
using Umbraco.Core.Persistence.Repositories;

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

            var redirects = Database.Query<Redirect>("SELECT * FROM Redirects");
            foreach (var redirect in redirects.Where(it => !it.IsRegex))
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
