using System;
using Umbraco.Core.Dashboards;

namespace Simple301.Core
{
    public class RedirectDashboard : IDashboard
    {
        public string[] Sections => new string[] { "Content" };

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();

        public string Alias => "redirectDashboard";

        public string View => "/App_Plugins/SimpleRedirects/app.html";
    }
}
