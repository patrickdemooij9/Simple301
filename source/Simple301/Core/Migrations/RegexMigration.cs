using Umbraco.Core.Migrations;

namespace Simple301.Core.Migrations
{
    public class RegexMigration : MigrationBase
    {
        public RegexMigration(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            if (!ColumnExists("Redirects", "IsRegex"))
            {
                Alter.Table("Redirects").AddColumn("IsRegex").AsBoolean().Nullable().Do();
            }
        }
    }
}
