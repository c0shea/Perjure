using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;

namespace Perjure
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PurgeRule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// Specifies the directory to purge, e.g. D:\Temp
        /// </summary>
        public string DirectoryPath { get; set; }

        /// <summary>
        /// Specifies the regular expression to match files, e.g. ^.*\.txt?$ matches files with .txt extension
        /// </summary>
        public string MatchPattern { get; set; }

        /// <summary>
        /// Specifies the number of days that a file must at least be in age in order to be purged
        /// </summary>
        public short DaysToPurgeAfter { get; set; }

        /// <summary>
        /// Specifies a minimum number of most recent files that should be kept even if they match all
        /// of the criteria (ignoring the <see cref="DaysToPurgeAfter"/>). For example, setting this to 5
        /// allows you to keep the most recent 5 files in this directory matching the <see cref="MatchPattern"/>
        /// but all others that are older than the <see cref="DaysToPurgeAfter"/> will be purged.
        /// </summary>
        public int? MinimumFilesToKeep { get; set; }

        /// <summary>
        /// Specifies whether or not sub-folders are included
        /// </summary>
        public bool IncludeSubfolders { get; set; }

        /// <summary>
        /// Specifies whether or not hidden files are included
        /// </summary>
        public bool IncludeHiddenFiles { get; set; }

        /// <summary>
        /// Specifies the type of file date to compare to the current system time when evaluating the <see cref="DaysToPurgeAfter"/>
        /// </summary>
        public TimeComparison TimeComparison { get; set; }

        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// Purges the matching files in DirectoryPath
        /// </summary>
        /// <returns>The number of files deleted from DirectoryPath</returns>
        public PurgeResult Process(DateTime compareToDate)
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

            var filesToPurge = FilesToPurge(compareToDate);

            if (filesToPurge.Count == 0)
            {
                Log.Debug("No files to purge in directory");
            }

            foreach (var file in filesToPurge)
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

        private List<FileInfo> FilesToPurge(DateTime compareToDate)
        {
            var allFilesMatchingName = AllFilesMatchingName();
            IEnumerable<FileInfo> filesToPurge = allFilesMatchingName;
            IEnumerable<FileInfo> filesToKeep = allFilesMatchingName;

            switch (TimeComparison)
            {
                case TimeComparison.Creation:
                    filesToPurge = filesToPurge.Where(f => (compareToDate - f.CreationTimeUtc).TotalDays > DaysToPurgeAfter);
                    filesToKeep = filesToPurge.OrderByDescending(f => f.CreationTimeUtc);
                    break;

                case TimeComparison.Write:
                    filesToPurge = filesToPurge.Where(f => (compareToDate - f.LastWriteTimeUtc).TotalDays > DaysToPurgeAfter);
                    filesToKeep = filesToPurge.OrderByDescending(f => f.LastWriteTimeUtc);
                    break;

                case TimeComparison.Access:
                    filesToPurge = filesToPurge.Where(f => (compareToDate - f.LastAccessTimeUtc).TotalDays > DaysToPurgeAfter);
                    filesToKeep = filesToPurge.OrderByDescending(f => f.LastAccessTimeUtc);
                    break;
            }

            filesToKeep = filesToKeep.Take(MinimumFilesToKeep ?? 0);

            return filesToPurge.Except(filesToKeep).ToList();
        }

        private List<FileInfo> AllFilesMatchingName()
        {
            var directory = new DirectoryInfo(DirectoryPath);
            var regexPattern = new Regex(MatchPattern ?? "");
            var allFiles = directory.GetFiles("*", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            var filesToPurge = allFiles.Where(f => !f.Attributes.HasFlag(FileAttributes.System) &&
                                                   regexPattern.IsMatch(f.Name));

            if (!IncludeHiddenFiles)
            {
                filesToPurge = filesToPurge.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
            }

            return filesToPurge.ToList();
        }
    }
}
