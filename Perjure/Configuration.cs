using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NLog;
using Perjure.PurgeRules;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Perjure
{
    public class Configuration
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public bool IncludeRecycleBin { get; set; }
        public bool IncludeTempDirectories { get; set; }
        public InternetExplorerPurgeOptions InternetExplorerPurgeOptions { get; set; }
        public List<FilePurgeRule> FilePurgeRules { get; set; }

        public IEnumerable<IPurgeRule> GetAllPurgeRules()
        {
            if (IncludeRecycleBin)
            {
                yield return new RecycleBinPurgeRule();
            }

            if (InternetExplorerPurgeOptions != InternetExplorerPurgeOptions.None)
            {
                yield return new InternetExplorerPurgeRule(InternetExplorerPurgeOptions);
            }

            if (IncludeTempDirectories)
            {
                FilePurgeRules.AddRange(BuildTempDirectoryFilePurgeRules());
            }

            if (FilePurgeRules != null)
            {
                foreach (var purgeRule in FilePurgeRules)
                {
                    yield return purgeRule;
                }
            }
        }

        private IEnumerable<FilePurgeRule> BuildTempDirectoryFilePurgeRules()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Log.Warn("Purging temporary directories is only supported on Windows.");
                yield break;
            }

            yield return BuildTempDirectoryFilePurgeRule(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"));

            var usersDirectory = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".."));
            var userTempDirectories = Directory.EnumerateDirectories(usersDirectory)
                .Select(d => Path.Combine(d, "AppData", "Local", "Temp"))
                .Where(Directory.Exists);

            foreach (var userTempDirectory in userTempDirectories)
            {
                yield return BuildTempDirectoryFilePurgeRule(userTempDirectory);
            }
        }

        private static FilePurgeRule BuildTempDirectoryFilePurgeRule(string tempDirectory)
        {
            return new FilePurgeRule
            {
                DirectoryPath = tempDirectory,
                DaysToPurgeAfter = 7,
                IncludeSubdirectories = true,
                IncludeHiddenFiles = false,
                DeleteEmptySubdirectories = true,
                DaysToPurgeEmptySubdirectoriesAfter = 7,
                TimeComparison = TimeComparison.LastAccess
            };
        }
    }
}
