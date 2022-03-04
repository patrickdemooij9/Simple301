using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleRedirects.Core.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace SimpleRedirects.Core.Middleware
{
    public class SimpleRedirectsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RedirectRepository _redirectRepository;
        private readonly IRuntimeState _runtimeState;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        private SimpleRedirectsOptions _config;

        public SimpleRedirectsMiddleware(RequestDelegate next,
            RedirectRepository redirectRepository,
            IRuntimeState runtimeState,
            IUmbracoContextAccessor umbracoContextAccessor,
            IOptionsMonitor<SimpleRedirectsOptions> options)
        {
            _next = next;
            _redirectRepository = redirectRepository;
            _runtimeState = runtimeState;
            _umbracoContextAccessor = umbracoContextAccessor;
            _config = options.CurrentValue;
            options.OnChange((newValue) => _config = newValue);
        }

        public async Task Invoke(HttpContext context)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                await _next.Invoke(context);
                return;
            }

            var pathAndQuery = context.Request.GetEncodedPathAndQuery();

            if (pathAndQuery.IndexOf("/umbraco", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                await _next(context);
                return;
            }

            if (_config.OnlyRedirectOn404)
            {
                context.Response.OnStarting(() =>
                {
                    if (context.Response.StatusCode != (int)HttpStatusCode.NotFound)
                    {
                        return Task.CompletedTask;
                    }

                    HandleRedirect(context);
                    return Task.CompletedTask;
                });
                await _next(context);
            }
            else
            {
                if (!HandleRedirect(context))
                {
                    await _next(context);
                }
            }
        }

        private bool HandleRedirect(HttpContext context)
        {
            var url = new Uri(context.Request.GetEncodedUrl());
            var matchedRedirect = _redirectRepository.FindRedirect(url);
            if (matchedRedirect == null)
            {
                return false;
            };

            var isPerm = matchedRedirect.RedirectCode == (int)HttpStatusCode.MovedPermanently;
            context.Response.Redirect(matchedRedirect.GetNewUrl(url), isPerm);
            return true;
        }
    }
}
