using SimpleRedirects.Core.Models;
using Umbraco.Core.Migrations;

namespace SimpleRedirects.Core.Migrations
{
    public class RedirectCodeMigration : MigrationBase
    {
        public RedirectCodeMigration(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            if (!ColumnExists("Redirects", "RedirectCode"))
            {
                Alter.Table("Redirects").AddColumn("RedirectCode").AsInt32().NotNullable().WithDefaultValue(301).Do();
            }
        }
    }
}
