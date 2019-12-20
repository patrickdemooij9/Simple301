using SimpleRedirects.Core.Models;
using Umbraco.Core.Migrations;

namespace SimpleRedirects.Core.Migrations
{
    public class InitialMigration : MigrationBase
    {
        public InitialMigration(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            if (!TableExists("Redirects"))
            {
                Create.Table<Redirect>().Do();
            }
        }
    }
}
