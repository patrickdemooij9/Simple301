namespace SimpleRedirects.Core.Options
{
    public class SimpleRedirectsOptions
    {
        public const string Position = "SimpleRedirects";

        public bool CacheEnabled {get; set;} = true;
        public int CacheDuration {get; set;} = 86400;
    }
}