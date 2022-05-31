using Microsoft.Extensions.Logging;
using SimpleRedirects.Core.Migrations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SimpleRedirects.Core.Components
{
    public class DatabaseUpgradeComponent : IComponent
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IKeyValueService _keyValueService;
        private readonly IRuntimeState _runtimeState;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;

        public DatabaseUpgradeComponent(IMigrationPlanExecutor migrationPlanExecutor, IScopeProvider scopeProvider, IKeyValueService keyValueService, IRuntimeState runtimeState)
        {
            _migrationPlanExecutor = migrationPlanExecutor;
            _scopeProvider = scopeProvider;
            _keyValueService = keyValueService;
            _runtimeState = runtimeState;
        }

        public void Initialize()
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return;

            var plan = new MigrationPlan("SimpleRedirectsMigration");
            plan.From(string.Empty)
                .To<InitialMigration>("state-1")
                .To<RegexMigration>("state-2")
                .To<RedirectCodeMigration>("state-3")
                .To<TrimOldUrlMigration>("state-4");

            var upgrader = new Upgrader(plan);
            upgrader.Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
        }

        public void Terminate()
        {

        }
    }
}