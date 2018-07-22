using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;

namespace Perjure
{
    public class PurgeRule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string DirectoryPath { get; set; }
        public string MatchPattern { get; set; }
        public short DaysToPurgeAfter { get; set; }
        public bool IncludeSubfolders { get; set; }
        public bool IncludeHiddenFiles { get; set; }

        /// <summary>
        /// Purges the matching files in DirectoryPath
        /// </summary>
        /// <returns>The number of files deleted from DirectoryPath</returns>
        public PurgeResult Process()
        {
            var result = new PurgeResult
            {
                WasDirectoryPurged = true,
                RuleExitCode = ExitCode.Success
            };

            if (!Directory.Exists(DirectoryPath))
            {
                result.FilesDeletedCount = 0;
                result.WasDirectoryPurged = false;
                result.RuleExitCode = ExitCode.DirectoryNotFound;
                
                Log.Error("Directory '{0}' was not found", DirectoryPath);

                return result;
            }

            Log.Info("Purging '{0}'", DirectoryPath);

            var directory = new DirectoryInfo(DirectoryPath);
            var regexPattern = new Regex(MatchPattern);
            var allFiles = directory.GetFiles("*", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            var filesToPurge = allFiles.Where(f => !f.Attributes.HasFlag(FileAttributes.System) &&
                                                   regexPattern.IsMatch(f.Name) &&
                                                   (DateTime.UtcNow - f.CreationTimeUtc).Days > DaysToPurgeAfter).ToList();

            if (!IncludeHiddenFiles)
            {
                filesToPurge = filesToPurge.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)).ToList();
            }

            if (filesToPurge.Count == 0)
            {
                Log.Debug("No files to purge in directory");
            }

            foreach (var file in filesToPurge.ToList())
            {
                try
                {
                    file.Delete();

                    Log.Debug("Deleted '{0}'", file.FullName);
                    result.FilesDeletedCount++;
                }
                catch (Exception ex)
                {
                    result.RuleExitCode = ExitCode.FileNotDeleted;
                    Log.Error(ex, "Failed to delete file '{0}'", file.FullName);
                }
            }

            return result;
        }
    }
}
