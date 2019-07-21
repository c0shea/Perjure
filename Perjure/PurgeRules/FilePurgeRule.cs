using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;

namespace Perjure.PurgeRules
{
    public class FilePurgeRule : IPurgeRule
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// Specifies the directory to purge, e.g. D:\Temp.
        /// </summary>
        public string DirectoryPath { get; set; }

        /// <summary>
        /// Specifies the regular expression to match files, e.g. ^.*\.txt?$ matches files with .txt extension.
        /// </summary>
        public string MatchPattern { get; set; }

        /// <summary>
        /// Specifies the number of days that a file must at least be in age in order to be purged.
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
        /// Specifies the maximum file size in bytes that should be kept. Files with sizes above this limit will always
        /// be purged, regardless of the other criteria. For example, setting this to 20971520 will always remove all files
        /// that are larger than 20 MB.
        /// </summary>
        public long? MaximumFileSizeInBytesToKeep { get; set; }

        /// <summary>
        /// Specifies whether or not sub-directories are included.
        /// </summary>
        public bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// Specifies whether or not hidden files are included.
        /// </summary>
        public bool IncludeHiddenFiles { get; set; }

        /// <summary>
        /// Specifies whether or not empty sub-directories are deleted after files have been purged.
        /// </summary>
        public bool DeleteEmptySubdirectories { get; set; }

        /// <summary>
        /// Specifies the number of days that a subdirectory must at least be in age in order to be purged.
        /// </summary>
        public short? DaysToPurgeEmptySubdirectoriesAfter { get; set; }

        /// <summary>
        /// Specifies the type of file date to compare to the current system time when evaluating the <see cref="DaysToPurgeAfter"/>.
        /// </summary>
        public TimeComparison TimeComparison { get; set; }

        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// Purges the matching files in DirectoryPath.
        /// </summary>
        /// <returns>The number of files deleted from DirectoryPath</returns>
        public void Process(DateTime compareToDate)
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Log.Error("Directory {DirectoryPath} was not found", DirectoryPath);
                return;
            }

            Log.Info("Purging files older than {DaysToPurgeAfter} days from {DirectoryPath}", DaysToPurgeAfter, DirectoryPath);

            var filesToPurge = FilesToPurge(compareToDate);
            var totalKilobytesPurged = 0m;

            foreach (var file in filesToPurge)
            {
                try
                {
                    file.Delete();
                    totalKilobytesPurged += file.Length / 1024.0m;

                    Log.Debug("Deleted file {FileFullName}", file.FullName);
                }
                catch (IOException ex) when (ex.Message.Contains("because it is being used by another process", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Warn("Failed to delete file {FileFullName} because it is being used by another process", file.FullName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to delete file {FileFullName}", file.FullName);
                }
            }

            ProcessEmptySubdirectories(DirectoryPath, compareToDate);

            Log.Info("Finished purging {TotalMegabytesPurged} MB from {DirectoryPath}", totalKilobytesPurged / 1024.0m, DirectoryPath);
        }

        private IEnumerable<FileInfo> FilesToPurge(DateTime compareToDate)
        {
            var allFilesMatchingName = AllFilesMatchingName().ToList();
            IEnumerable<FileInfo> filesToPurge = allFilesMatchingName;
            IEnumerable<FileInfo> filesToKeep = allFilesMatchingName;

            switch (TimeComparison)
            {
                case TimeComparison.Creation:
                    filesToPurge = filesToPurge.Where(f => (compareToDate - f.CreationTimeUtc).TotalDays > DaysToPurgeAfter);
                    filesToKeep = filesToKeep.OrderByDescending(f => f.CreationTimeUtc);
                    break;

                case TimeComparison.LastWrite:
                    filesToPurge = filesToPurge.Where(f => (compareToDate - f.LastWriteTimeUtc).TotalDays > DaysToPurgeAfter);
                    filesToKeep = filesToKeep.OrderByDescending(f => f.LastWriteTimeUtc);
                    break;

                case TimeComparison.LastAccess:
                    filesToPurge = filesToPurge.Where(f => (compareToDate - f.LastAccessTimeUtc).TotalDays > DaysToPurgeAfter);
                    filesToKeep = filesToKeep.OrderByDescending(f => f.LastAccessTimeUtc);
                    break;
            }

            filesToKeep = filesToKeep.Take(MinimumFilesToKeep ?? 0);

            // Maximum file size always overrides all other criteria
            if (MaximumFileSizeInBytesToKeep.HasValue)
            {
                var filesExceedingMaximumSize = allFilesMatchingName.Where(f => f.Length > MaximumFileSizeInBytesToKeep.Value).ToList();
                filesToPurge = filesToPurge.Concat(filesExceedingMaximumSize);
                filesToKeep = filesToKeep.Except(filesExceedingMaximumSize);
            }

            return filesToPurge.Except(filesToKeep);
        }

        private IEnumerable<FileInfo> AllFilesMatchingName()
        {
            var directory = new DirectoryInfo(DirectoryPath);
            var regexPattern = new Regex(MatchPattern ?? "");

            var allFiles = directory.EnumerateFiles("*", IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            var filesToPurge = allFiles.Where(f => !f.Attributes.HasFlag(FileAttributes.System) &&
                                                   regexPattern.IsMatch(f.Name));

            if (!IncludeHiddenFiles)
            {
                filesToPurge = filesToPurge.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
            }

            return filesToPurge;
        }

        private void ProcessEmptySubdirectories(string baseDirectory, DateTime compareToDate)
        {
            if (!DeleteEmptySubdirectories)
            {
                return;
            }

            foreach (var directory in Directory.EnumerateDirectories(baseDirectory))
            {
                if (IncludeSubdirectories)
                {
                    ProcessEmptySubdirectories(directory, compareToDate);
                }

                if (Directory.EnumerateFileSystemEntries(directory).Any())
                {
                    Log.Trace("Skipping subdirectory {Directory} since it's not empty", directory);

                    continue;
                }

                if (DaysToPurgeEmptySubdirectoriesAfter.HasValue)
                {
                    switch (TimeComparison)
                    {
                        case TimeComparison.Creation:
                            if ((compareToDate - Directory.GetCreationTimeUtc(directory)).TotalDays > DaysToPurgeEmptySubdirectoriesAfter)
                            {
                                Log.Trace("Skipping subdirectory {Directory} since it was created within the last {DaysToPurgeEmptySubdirectoriesAfter} days",
                                    directory,
                                    DaysToPurgeEmptySubdirectoriesAfter);

                                continue;
                            }

                            break;

                        case TimeComparison.LastWrite:
                            if ((compareToDate - Directory.GetLastWriteTimeUtc(directory)).TotalDays > DaysToPurgeEmptySubdirectoriesAfter)
                            {
                                Log.Trace("Skipping subdirectory {Directory} since it was written to within the last {DaysToPurgeEmptySubdirectoriesAfter} days",
                                    directory,
                                    DaysToPurgeEmptySubdirectoriesAfter);

                                continue;
                            }

                            break;

                        case TimeComparison.LastAccess:
                            if ((compareToDate - Directory.GetLastAccessTimeUtc(directory)).TotalDays > DaysToPurgeEmptySubdirectoriesAfter)
                            {
                                Log.Trace("Skipping subdirectory {Directory} since it was accessed within the last {DaysToPurgeEmptySubdirectoriesAfter} days",
                                    directory,
                                    DaysToPurgeEmptySubdirectoriesAfter);

                                continue;
                            }

                            break;
                    }
                }

                try
                {
                    Directory.Delete(directory);

                    Log.Debug("Deleted empty subdirectory {Directory}", directory);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to delete empty subdirectory {Directory}", directory);
                }
            }
        }
    }
}
