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

        internal void Process() {
            Console.WriteLine(DirectoryPath);
            for (int i = 0; i < DirectoryPath.Length; i++) {
                Console.Write("=");
            }
            Console.WriteLine();

            var directory = new DirectoryInfo(DirectoryPath);

            var allFiles = directory.GetFiles("*", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            var regexPattern = new Regex(MatchPattern);

            var filesToPurge = allFiles.Where(f => !f.Attributes.HasFlag(FileAttributes.System) &&
                                                   regexPattern.IsMatch(f.Name) &&
                                                   (DateTime.UtcNow - f.CreationTimeUtc).Days > DaysToPurgeAfter);

            if (!IncludeHiddenFiles) {
                filesToPurge = filesToPurge.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
            }

            foreach (var file in filesToPurge.ToList()) {
                file.Delete();
                Console.WriteLine($"Deleted {file.FullName}");
            }
        }
    }
}