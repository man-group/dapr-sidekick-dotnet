using System;

namespace Dapr.Sidekick.Http
{
    public static class UriHelper
    {
        public static Uri Parse(string uri) =>
            string.IsNullOrEmpty(uri) ? null :
            Uri.TryCreate(AdjustUriHost(uri), UriKind.Absolute, out var result) ? result :
            null;

        private static string AdjustUriHost(string uri)
        {
            if (uri.Contains("*:"))
            {
                return uri.Replace("*:", "127.0.0.1:");
            }
            else if (uri.Contains("+:"))
            {
                return uri.Replace("+:", "127.0.0.1:");
            }
            else
            {
                return uri;
            }
        }
    }
}
