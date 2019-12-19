using Umbraco.Web.Routing;
using System.Linq;

namespace Simple301.Core
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
            //Get the requested URL path + query
            var path = request.Uri.PathAndQuery.ToLower();

            //Check the table
            var matchedRedirect = _repository.FindRedirect(path);
            if (matchedRedirect == null) return false;

            //Found one, set the 301 redirect on the request and return
            request.SetRedirectPermanent(matchedRedirect.NewUrl);
            return true;
        }
    }
}
