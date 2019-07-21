using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NLog;
using Perjure.Interop;

namespace Perjure.PurgeRules
{
    public class InternetExplorerPurgeRule : IPurgeRule
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ClearMyTracksByProcessOptions _options;

        public InternetExplorerPurgeRule(InternetExplorerPurgeOptions options)
        {
            _options |= ClearMyTracksByProcessOptions.DontShowGui;

            if (options.HasFlag(InternetExplorerPurgeOptions.TemporaryInternetFiles))
            {
                _options |= ClearMyTracksByProcessOptions.ClearTemporaryInternetFiles;
            }

            if (options.HasFlag(InternetExplorerPurgeOptions.Cookies))
            {
                _options |= ClearMyTracksByProcessOptions.ClearCookies;
            }

            if (options.HasFlag(InternetExplorerPurgeOptions.History))
            {
                _options |= ClearMyTracksByProcessOptions.ClearHistory;
            }

            if (options.HasFlag(InternetExplorerPurgeOptions.DownloadHistory))
            {
                _options |= ClearMyTracksByProcessOptions.ClearOfflineFavoritesAndDownloadHistory;
            }

            if (options.HasFlag(InternetExplorerPurgeOptions.FormData))
            {
                _options |= ClearMyTracksByProcessOptions.ClearFormData;
            }

            if (options.HasFlag(InternetExplorerPurgeOptions.Passwords))
            {
                _options |= ClearMyTracksByProcessOptions.ClearPasswords;
            }

            if (options.HasFlag(InternetExplorerPurgeOptions.TrackingProtection))
            {
                _options |= ClearMyTracksByProcessOptions.ClearTrackingData;
            }
        }

        public void Process(DateTime compareToDate)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Log.Warn("Clearing Internet Explorer browsing history is only supported on Windows.");
                return;
            }

            Log.Info("Clearing Internet Explorer browsing history with options {Options}", (int)_options);

            using (var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "rundll32.exe",
                    Arguments = $"inetcpl.cpl,ClearMyTracksByProcess {(int)_options}"
                }
            })
            {
                process.Start();
                process.WaitForExit();
            }

            Log.Info("Finished clearing Internet Explorer browser history");
        }
    }
}
