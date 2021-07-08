using SimpleRedirects.Core.Models;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SimpleRedirects.Core.Migrations
{
    public class InitialMigration : MigrationBase
    {
        public InitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("Redirects"))
            {
                Create.Table<Redirect>().Do();
            }
        }
    }
}