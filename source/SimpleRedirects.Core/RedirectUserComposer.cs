using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleRedirects.Core.Components;
using SimpleRedirects.Core.Options;
using SimpleRedirects.Core.Utilities.Caching;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace SimpleRedirects.Core
{
    public class RedirectUserComposer : IUserComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Dashboards().Add<RedirectDashboard>();
            builder.Components().Append<DatabaseUpgradeComponent>();
            builder.ContentFinders().InsertBefore<ContentFinderByUrl, RedirectContentFinder>();

            builder.Services.AddUnique<RedirectRepository>();
            builder.Services.AddUnique<ICacheManager, CacheManager>();

            builder.Services.Configure<SimpleRedirectsOptions>(builder.Config.GetSection(
                                        SimpleRedirectsOptions.Position));
        }
    }
}