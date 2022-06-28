using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace SimpleRedirects.Core.Notifications
{
    public class RedirectCacheRefresherNotification : CacheRefresherNotification
    {
        public RedirectCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
