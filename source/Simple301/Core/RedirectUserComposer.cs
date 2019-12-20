using Simple301.Core.Components;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Simple301.Core
{
    public class RedirectUserComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Dashboards().Add<RedirectDashboard>();
            composition.Components().Append<DatabaseUpgradeComponent>();
            composition.ContentFinders().InsertBefore<ContentFinderByUrl, RedirectContentFinder>();
            composition.Register(typeof(RedirectRepository), Lifetime.Request);
        }
    }
}
