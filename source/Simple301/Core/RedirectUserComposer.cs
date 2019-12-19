using Simple301.Core.Components;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;

namespace Simple301.Core
{
    public class RedirectUserComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Dashboards().Add<RedirectDashboard>();
            composition.Components().Append<DatabaseUpgradeComponent>();
            composition.ContentFinders().Append<RedirectContentFinder>();
        }
    }
}
