using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Perjure.PurgeRules
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InternetExplorerPurgeOptions
    {
        None = 0,
        TemporaryInternetFiles = 1,
        Cookies = 2,
        History = 4,
        DownloadHistory = 8,
        FormData = 16,
        Passwords = 32,
        TrackingProtection = 64
    }
}
