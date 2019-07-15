using System;

namespace Perjure.Interop
{
    /// <remarks>
    /// https://github.com/SeleniumHQ/selenium/commit/92c0da7299ea040fdc6d7d287296c397bc5c9fca#diff-cf132f5561101876cdfe807f013ddddcR61
    /// </remarks>
    [Flags]
    internal enum ClearMyTracksByProcessOptions
    {
        ClearHistory = 0x0001,
        ClearCookies = 0x0002,
        ClearTemporaryInternetFiles = 0x0004,
        ClearOfflineFavoritesAndDownloadHistory = 0x0008,
        ClearFormData = 0x0010,
        ClearPasswords = 0x0020,
        ClearPhishingFilterData = 0x0040,
        ClearWebpageRecoveryData = 0x0080,
        ClearTrackingData = 0x0800,
        DontShowGui = 0x0100,
        NoMultithreading = 0x0200,
        ClearPrivateBrowsingModeCache = 0x0400,
        ClearAddOnData = 0x1000,
        PreserveCachedDataForFavoriteWebsites = 0x2000
    }
}
