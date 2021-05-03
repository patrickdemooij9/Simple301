using Microsoft.Extensions.Logging;
using SimpleRedirects.Core.Migrations;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace SimpleRedirects.Core.Components {
    public class DatabaseUpgradeComponent : IComponent {
        private readonly IScopeProvider _scopeProvider;
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly IKeyValueService _keyValueService;
        private readonly ILogger<Upgrader> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public DatabaseUpgradeComponent (IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, ILogger<Upgrader> logger, ILoggerFactory loggerFactory) {
            _loggerFactory = loggerFactory;
            _scopeProvider = scopeProvider;
            _migrationBuilder = migrationBuilder;
            _keyValueService = keyValueService;
            _logger = logger;
        }

        public void Initialize () {
            var plan = new MigrationPlan ("SimpleRedirectsMigration");
            plan.From (string.Empty)
                .To<InitialMigration> ("state-1")
                .To<RegexMigration> ("state-2")
                .To<RedirectCodeMigration> ("state-3")
                .To<TrimOldUrlMigration> ("state-4");

            var upgrader = new Upgrader (plan);
            upgrader.Execute (_scopeProvider, _migrationBuilder, _keyValueService, _logger, _loggerFactory);
        }

        public void Terminate () {

        }
    }
}