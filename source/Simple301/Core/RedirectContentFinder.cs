using System;
using Umbraco.Web.Routing;
using System.Linq;
using System.Net;
using SimpleRedirects.Core.Utilities;
using Umbraco.Core.Composing;

namespace SimpleRedirects.Core
{
    /// <summary>
    /// Content finder to be injected at the start of the Umbraco pipeline that first
    /// looks for any redirects that path the path + query
    /// </summary>
    public class RedirectContentFinder : IContentFinder
    {
        private readonly RedirectRepository _repository;

        private readonly bool _ignoreQueryString;
        private readonly bool _preserveQueryString;

        public RedirectContentFinder(RedirectRepository repository)
        {
            _repository = repository;

            var settingsUtility = new SettingsUtility();
            _ignoreQueryString = settingsUtility.AppSettingExists(SettingsKeys.IgnoreQueryString) && settingsUtility.GetAppSetting<bool>(SettingsKeys.IgnoreQueryString);
            _preserveQueryString = settingsUtility.AppSettingExists(SettingsKeys.PreserveQueryString) && settingsUtility.GetAppSetting<bool>(SettingsKeys.PreserveQueryString);
        }

        public bool TryFindContent(PublishedRequest request)
        {
            var uri = request.Uri;
            if (_ignoreQueryString)
            {
                uri = new Uri(uri.AbsoluteUri.Split('?').First());
            }

            //Check the table
            var matchedRedirect = _repository.FindRedirect(uri);
            if (matchedRedirect == null) return false;

            request.SetRedirect(matchedRedirect.GetNewUrl(request.Uri, _preserveQueryString), matchedRedirect.RedirectCode);

            return true;
        }
    }
}
