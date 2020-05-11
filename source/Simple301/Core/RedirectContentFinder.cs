using Umbraco.Web.Routing;
using System.Linq;
using System.Net;
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

        public RedirectContentFinder(RedirectRepository repository)
        {
            _repository = repository;
        }

        public bool TryFindContent(PublishedRequest request)
        {
            //Check the table
            Current.Logger.Info(typeof(RedirectContentFinder), "Request: " + request.Uri.ToString());
            var matchedRedirect = _repository.FindRedirect(request.Uri);
            if (matchedRedirect == null) return false;

            request.SetRedirect(matchedRedirect.GetNewUrl(request.Uri), matchedRedirect.RedirectCode);

            return true;
        }
    }
}
