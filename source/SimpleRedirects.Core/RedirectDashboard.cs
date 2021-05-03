using System;
using Umbraco.Cms.Core.Dashboards;

namespace SimpleRedirects.Core
{
    public class RedirectDashboard : IDashboard
    {
        public string[] Sections => new string[] { "Content" };

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();

        public string Alias => "redirectDashboard";

        public string View => "/App_Plugins/SimpleRedirects/app.html";
    }
}