using SimpleRedirects.Core.Components;
using SimpleRedirects.Core.Utilities.Caching;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace SimpleRedirects.Core
{
    [RuntimeLevel(MinLevel =RuntimeLevel.Run)]
    public class RedirectUserComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Dashboards().Add<RedirectDashboard>();
            composition.Components().Append<DatabaseUpgradeComponent>();
            composition.ContentFinders().InsertBefore<ContentFinderByUrl, RedirectContentFinder>();
            composition.Register(typeof(RedirectRepository), Lifetime.Request);
            composition.Register(typeof(ICacheManager), typeof(CacheManager));
        }
    }
}
