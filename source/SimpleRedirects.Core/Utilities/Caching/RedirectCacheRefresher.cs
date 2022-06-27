using System;
using SimpleRedirects.Core.Notifications;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace SimpleRedirects.Core.Utilities.Caching
{
    public class RedirectCacheRefresher : CacheRefresherBase<RedirectCacheRefresherNotification>
    {
        public static readonly Guid UniqueId = Guid.Parse("fe3847bc-80c4-4ce0-abde-551bb409599a");

        public RedirectCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory) : base(appCaches, eventAggregator, factory)
        {
        }

        public override Guid RefresherUniqueId => UniqueId;
        public override string Name => "Redirects Cache Refresher";

        public override void RefreshAll()
        {
            AppCaches.RuntimeCache.ClearByKey(RedirectRepository.CacheCategoryKey);
            base.RefreshAll();
        }
    }
}
