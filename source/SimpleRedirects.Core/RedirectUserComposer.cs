using System.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleRedirects.Core.Components;
using SimpleRedirects.Core.Middleware;
using SimpleRedirects.Core.Options;
using SimpleRedirects.Core.Services;
using SimpleRedirects.Core.Utilities.Caching;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;

namespace SimpleRedirects.Core
{
    public class RedirectUserComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Dashboards().Add<RedirectDashboard>();
            builder.Components().Append<DatabaseUpgradeComponent>();

            builder.Services.AddSingleton<RedirectRepository>();
            builder.Services.AddSingleton<ICacheManager, CacheManager>();

            builder.Services.AddScoped<ImportExportFactory>();
            builder.Services.AddScoped<CsvImportExportService>()
                .AddScoped<IImportExportService, CsvImportExportService>(s => s.GetService<CsvImportExportService>());
            builder.Services.AddScoped<ExcelImportExportService>()
                .AddScoped<IImportExportService, ExcelImportExportService>(s => s.GetService<ExcelImportExportService>());

            builder.Services.Configure<SimpleRedirectsOptions>(builder.Config.GetSection(
                SimpleRedirectsOptions.Position));
            var onlyRedirectOn404 = builder.Config.GetSection(SimpleRedirectsOptions.Position)
                ?.Get<SimpleRedirectsOptions>()?.OnlyRedirectOn404 ?? false;

            builder.Services.Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(new UmbracoPipelineFilter(
                    "SimpleRedirects",
                    applicationBuilder =>
                    {
                        if (!onlyRedirectOn404)
                            applicationBuilder.UseMiddleware<SimpleRedirectsMiddleware>();
                    },
                    applicationBuilder =>
                    {
                        if (onlyRedirectOn404)
                            applicationBuilder.UseMiddleware<SimpleRedirectsMiddleware>();
                    },
                    applicationBuilder => { }
                ));
            });
        }
    }
}