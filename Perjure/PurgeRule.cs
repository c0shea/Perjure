using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Perjure {
    public class PurgeRule {
        public string DirectoryPath { get; set; }
        public string MatchPattern { get; set; }
        public short DaysToPurgeAfter { get; set; }
        public bool IncludeSubfolders { get; set; }
        public bool IncludeHiddenFiles { get; set; }

        /// <summary>
        /// Purges the matching files in DirectoryPath
        /// </summary>
        /// <returns>The number of files deleted from DirectoryPath</returns>
        internal Statistic Process() {
            var statistic = new Statistic {
                WasDirectoryPurged = true,
                RuleExitCode = ExitCode.Success
            };

            if (!Directory.Exists(DirectoryPath)) {
                statistic.FilesDeletedCount = 0;
                statistic.WasDirectoryPurged = false;
                statistic.RuleExitCode = ExitCode.DirectoryNotFound;
                Console.WriteLine($"\nERROR: The directory \"{DirectoryPath}\" was not found.");
                return statistic;
            }

            Console.WriteLine(DirectoryPath);
            int maxWidth = DirectoryPath.Length > 80 ? 80 : DirectoryPath.Length;
            for (int i = 0; i < maxWidth; i++) {
                Console.Write("=");
            }
            Console.WriteLine();

            var directory = new DirectoryInfo(DirectoryPath);

            var allFiles = directory.GetFiles("*", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            var regexPattern = new Regex(MatchPattern);

            var filesToPurge = allFiles.Where(f => !f.Attributes.HasFlag(FileAttributes.System) &&
                                                   regexPattern.IsMatch(f.Name) &&
                                                   (DateTime.UtcNow - f.CreationTimeUtc).Days > DaysToPurgeAfter).ToList();

            if (!IncludeHiddenFiles) {
                filesToPurge = filesToPurge.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)).ToList();
            }

            if (filesToPurge.Count == 0) {
                Console.WriteLine("No files to purge in directory.");
            }

            foreach (var file in filesToPurge.ToList()) {
                try {
                    file.Delete();
                    Console.WriteLine($"Deleted {file.FullName}");
                    statistic.FilesDeletedCount++;
                }
                catch (Exception ex) {
                    statistic.RuleExitCode = ExitCode.FileNotDeleted;
                    Console.WriteLine($"ERROR: Couldn't delete file \"{file.FullName}\". {ex.Message}");
                }
            }

            return statistic;
        }
    }
}