using Umbraco.Cms.Core.Routing;

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

        public bool TryFindContent(IPublishedRequestBuilder request)
        {
            //Check the table
            var matchedRedirect = _repository.FindRedirect(request.Uri);
            if (matchedRedirect == null) return false;

            request.SetRedirect(matchedRedirect.GetNewUrl(request.Uri), matchedRedirect.RedirectCode);

            return true;
        }
    }
}