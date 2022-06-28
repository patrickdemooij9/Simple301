using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text.RegularExpressions;
using SimpleRedirects.Core.Extensions;
using SimpleRedirects.Core.Models;
using SimpleRedirects.Core.Utilities.Caching;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SimpleRedirects.Core
{
    public class RedirectRepository
    {
        private readonly ICacheManager _cacheManager;
        private readonly DistributedCache _distributedCache;
        public const string CacheCategoryKey = "Redirects";

        private readonly IScopeProvider _scopeProvider;

        public RedirectRepository(IScopeProvider scopeProvider, ICacheManager cacheManager, DistributedCache distributedCache)
        {
            _scopeProvider = scopeProvider;
            _cacheManager = cacheManager;
            _distributedCache = distributedCache;
        }

        /// <summary>
        /// Get all redirects from the repositry
        /// </summary>
        /// <returns>Collection of redirects</returns>
        public IEnumerable<Redirect> GetAllRedirects()
        {
            // Update with latest from DB
            return FetchRedirects().Select(x => x.Value);
        }

        /// <summary>
        /// Add a new redirect to the redirects collection
        /// </summary>
        /// <param name="oldUrl">Old Url to redirect from</param>
        /// <param name="newUrl">New Url to redirect to</param>
        /// <param name="notes">Any associated notes with this redirect</param>
        /// <returns>New redirect from DB if successful</returns>
        public Redirect AddRedirect(bool isRegex, string oldUrl, string newUrl, int redirectCode, string notes)
        {
            if (!oldUrl.IsSet()) throw new ArgumentNullException("oldUrl");
            if (!newUrl.IsSet()) throw new ArgumentNullException("newUrl");

            //Check if type is a valid redirect response status
            if (!redirectCode.Equals((int)HttpStatusCode.Redirect) && !redirectCode.Equals((int)HttpStatusCode.MovedPermanently))
                throw new ArgumentException("You can only choose the 301 & 302 status code!");

            if (!isRegex)
                oldUrl = CleanUrl(oldUrl);

            //Ensure starting slash if not regex
            if (!isRegex && Uri.IsWellFormedUriString(oldUrl, UriKind.Relative))
                oldUrl = oldUrl.EnsurePrefix("/");

            // Allow external redirects and ensure slash if not absolute
            newUrl = Uri.IsWellFormedUriString(newUrl, UriKind.Absolute) ?
                newUrl :
                newUrl.EnsurePrefix("/").ToLower();

            // First look for single match
            var redirect = FetchRedirectByOldUrl(oldUrl);
            if (redirect != null) throw new ArgumentException("A redirect for " + oldUrl + " already exists");

            // Second pull all for loop detection
            var redirects = FetchRedirects();
            if (!isRegex && DetectLoop(oldUrl, newUrl, redirects)) throw new ApplicationException("Adding this redirect would cause a redirect loop");

            int idObj;

            //Add redirect to DB
            using (var scope = _scopeProvider.CreateScope())
            {
                idObj = Convert.ToInt32(scope.Database.Insert(new Redirect
                {
                    IsRegex = isRegex,
                    OldUrl = oldUrl,
                    NewUrl = newUrl,
                    RedirectCode = redirectCode,
                    LastUpdated = DateTime.Now.ToUniversalTime(),
                    Notes = notes
                }));

                scope.Complete();
            }

            //Clear the current cache
            ClearCache();

            //Fetch the added redirect
            var newRedirect = FetchRedirectById(Convert.ToInt32(idObj));

            //return new redirect
            return newRedirect;
        }

        /// <summary>
        /// Update a given redirect
        /// </summary>
        /// <param name="redirect">Redirect to update</param>
        /// <returns>Updated redirect if successful</returns>
        public Redirect UpdateRedirect(Redirect redirect)
        {
            if (redirect == null) throw new ArgumentNullException("redirect");
            if (!redirect.OldUrl.IsSet()) throw new ArgumentNullException("redirect.OldUrl");
            if (!redirect.NewUrl.IsSet()) throw new ArgumentNullException("redirect.NewUrl");

            if (!redirect.IsRegex)
                redirect.OldUrl = CleanUrl(redirect.OldUrl);

            //Check if type is a valid redirect response status
            if (!redirect.RedirectCode.Equals((int)HttpStatusCode.Redirect) && !redirect.RedirectCode.Equals((int)HttpStatusCode.MovedPermanently))
                throw new ArgumentException("You can only choose the 301 & 302 status code!");

            //Ensure starting slash
            if (!redirect.IsRegex && Uri.IsWellFormedUriString(redirect.OldUrl, UriKind.Relative))
                redirect.OldUrl = redirect.OldUrl.EnsurePrefix("/").ToLower();

            // Allow external redirects and ensure slash if not absolute
            redirect.NewUrl = Uri.IsWellFormedUriString(redirect.NewUrl, UriKind.Absolute) ?
                redirect.NewUrl :
                redirect.NewUrl.EnsurePrefix("/").ToLower();

            // First check if a single existing redirect
            var existingRedirect = FetchRedirectByOldUrl(redirect.OldUrl);
            if (existingRedirect != null && existingRedirect.Id != redirect.Id) throw new ArgumentException("A redirect for " + redirect.OldUrl + " already exists");

            // Second pull all for loop detection
            var redirects = FetchRedirects();
            if (!redirect.IsRegex && DetectLoop(redirect.OldUrl, redirect.NewUrl, redirects)) throw new ApplicationException("Adding this redirect would cause a redirect loop");

            //get DB Context, set update time, and persist
            using (var scope = _scopeProvider.CreateScope())
            {
                redirect.LastUpdated = DateTime.Now.ToUniversalTime();
                scope.Database.Update(redirect);
                scope.Complete();

                //Clear the current cache
                ClearCache();

                //return updated redirect
                return redirect;
            }
        }

        /// <summary>
        /// Handles deleting a redirect from the redirect collection
        /// </summary>
        /// <param name="id">Id of redirect to remove</param>
        public void DeleteRedirect(int id)
        {
            var item = FetchRedirectById(id);
            if (item == null) throw new ArgumentException("No redirect with an Id that matches " + id);

            //Get database context and delete
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Delete(item);
                scope.Complete();
            }

            //Clear the current cache
            ClearCache();
        }

        /// <summary>
        /// Handles finding a redirect based on the oldUrl
        /// </summary>
        /// <param name="oldUrl">Url to search for</param>
        /// <returns>Matched Redirect</returns>
        public Redirect FindRedirect(Uri oldUrl)
        {
            var matchedRedirect = FetchRedirectByOldUri(oldUrl, fromCache: true);
            if (matchedRedirect != null) return matchedRedirect;

            // fetch regex redirects
            var regexRedirects = FetchRegexRedirects(fromCache: true);

            foreach (var regexRedirect in regexRedirects)
            {
                if (Regex.IsMatch(oldUrl.AbsoluteUri, regexRedirect.OldUrl)) return regexRedirect;
            }

            return null;
        }

        /// <summary>
        /// Handles clearing the cache
        /// </summary>
        public void ClearCache()
        {
            _distributedCache.RefreshAll(RedirectCacheRefresher.UniqueId);
        }

        /// <summary>
        /// Fetches all redirects through cache layer
        /// </summary>
        /// <returns>Collection of redirects</returns>
        private Dictionary<string, Redirect> FetchRedirects(bool fromCache = false)
        {
            // if from cache, make sure we add if it doesn't exist
            return fromCache
                ? _cacheManager.GetCacheItem($"{CacheCategoryKey}_All", FetchRedirectsFromDb)
                : FetchRedirectsFromDb();
        }

        /// <summary>
        /// Fetches a single redirect from the DB based on an Id
        /// </summary>
        /// <param name="id">Id of redirect to fetch</param>
        /// <returns>Single redirect with matching Id</returns>
        private Redirect FetchRedirectById(int id, bool fromCache = false)
        {
            return fromCache
                ? _cacheManager.GetCacheItem($"{CacheCategoryKey}_id_{id}", () => FetchRedirectFromDbByQuery(x => x.Id == id))
                : FetchRedirectFromDbByQuery(x => x.Id == id);
        }

        /// <summary>
        /// Fetches a single redirect from the DB based on OldUrl
        /// </summary>
        /// <param name="oldUrl">OldUrl of redirect to find</param>
        /// <returns>Single redirect with matching OldUrl</returns>
        private Redirect FetchRedirectByOldUrl(string oldUrl, bool fromCache = false)
        {
            oldUrl = CleanUrl(oldUrl);
            return fromCache
                ? _cacheManager.GetCacheItem($"{CacheCategoryKey}_oldUrl_{oldUrl}",
                    () => FetchRedirectFromDbByQuery(x => x.OldUrl == oldUrl))
                : FetchRedirectFromDbByQuery(x => x.OldUrl == oldUrl);
        }

        private Redirect FetchRedirectByOldUri(Uri oldUrl, bool fromCache = false)
        {
            var absoluteUri = CleanUrl(oldUrl.AbsoluteUri);
            var pathAndQuery = CleanUrl(oldUrl.PathAndQuery);
            return fromCache
                ? _cacheManager.GetCacheItem($"{CacheCategoryKey}_uriRedirects_{oldUrl.AbsoluteUri}",
                    () => FetchRedirectFromDbByQuery(x =>
                        x.OldUrl == absoluteUri || x.OldUrl == pathAndQuery))
                : FetchRedirectFromDbByQuery(x => x.OldUrl == absoluteUri || x.OldUrl == pathAndQuery);
        }

        /// <summary>
        /// Fetches the list of Regex redirects from the DB or cache
        /// </summary>
        /// <param name="fromCache">Set to pull from cache</param>
        /// <returns>Collection or regex redirects</returns>
        private List<Redirect> FetchRegexRedirects(bool fromCache = false)
        {
            return fromCache
                ? _cacheManager.GetCacheItem($"{CacheCategoryKey}_regexRedirects",
                    () => FetchRedirectsFromDbByQuery(x => x.IsRegex).ToList())
                : FetchRedirectsFromDbByQuery(x => x.IsRegex).ToList();
        }

        /// <summary>
        /// Handles fetching a single redirect from the DB based
        /// on the provided query and parameter
        /// </summary>
        /// <returns>Redirect</returns>
        private Redirect FetchRedirectFromDbByQuery(Expression<Func<Redirect, bool>> expression)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                return scope.Database.FirstOrDefault<Redirect>(scope.SqlContext.Sql().From<Redirect>().Where(expression));
            }
        }

        /// <summary>
        /// Handles fetching a collection of redirects from the DB based
        /// on the provided query and parameter
        /// </summary>
        /// <returns>Collection of redirects</returns>
        private IEnumerable<Redirect> FetchRedirectsFromDbByQuery(Expression<Func<Redirect, bool>> expression)
        {
            IEnumerable<Redirect> results;
            using (var scope = _scopeProvider.CreateScope())
            {
                results = scope.Database.Fetch<Redirect>
                (scope.SqlContext.Sql()
                    .SelectAll()
                    .From<Redirect>()
                    .Where(expression)
                );

                scope.Complete();
            }
            return results;
        }

        /// <summary>
        /// Fetches all redirects from the database
        /// </summary>
        /// <returns>Collection of redirects</returns>
        private Dictionary<string, Redirect> FetchRedirectsFromDb()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var redirects = scope.Database.Query<Redirect>("SELECT * FROM Redirects");
                return redirects != null ? redirects.ToDictionary(x => x.OldUrl) : new Dictionary<string, Redirect>();
            }
        }

        private string CleanUrl(string url)
        {
            var urlParts = url.ToLowerInvariant().Split('?');
            var baseUrl = urlParts[0].TrimEnd('/');
            return urlParts.Length == 1 ? baseUrl : $"{baseUrl}?{string.Join("?", urlParts.Skip(1))}";
        }

        /// <summary>
        /// Detects a loop in the redirects list given the new redirect.
        /// Uses Floyd's cycle-finding algorithm.
        /// </summary>
        /// <param name="oldUrl">Old URL for new redirect</param>
        /// <param name="newUrl">New URL for new redirect</param>
        /// <param name="redirects">Current list of all redirects</param>
        /// <returns>True if loop detected, false if no loop detected</returns>
        private bool DetectLoop(string oldUrl, string newUrl, Dictionary<string, Redirect> redirects)
        {
            // quick check for any links to this new redirect
            if (!redirects.ContainsKey(newUrl) && !redirects.Any(x => x.Value.NewUrl.Equals(oldUrl))) return false;

            // clone redirect list
            var linkedList = redirects.ToDictionary(entry => entry.Key, entry => entry.Value);
            var redirect = new Redirect() { OldUrl = oldUrl, NewUrl = newUrl };

            // add new redirect to cloned list for traversing
            if (!linkedList.ContainsKey(oldUrl))
                linkedList.Add(oldUrl, redirect);
            else
                linkedList[oldUrl] = redirect;

            // Use Floyd's cycle finding algorithm to detect loops in a linked list
            var slowP = redirect;
            var fastP = redirect;

            while (slowP != null && fastP != null && linkedList.ContainsKey(fastP.NewUrl))
            {
                slowP = linkedList[slowP.NewUrl];
                fastP = linkedList.ContainsKey(linkedList[fastP.NewUrl].NewUrl) ? linkedList[linkedList[fastP.NewUrl].NewUrl] : null;

                if (slowP == fastP)
                {
                    return true;
                }
            }
            return false;
        }
    }
}