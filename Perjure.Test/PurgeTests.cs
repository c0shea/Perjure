using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Perjure.Test
{
    [TestClass]
    public class PurgeTests : BaseTest
    {
        [TestMethod]
        public void DeleteAllFilesBeforeYesterday()
        {
            var rule = new PurgeRule
            {
                DirectoryPath = TempPath,
                DaysToPurgeAfter = 1
            };
            
            Assert.AreEqual(StartingFileCount, FileCount);

            var result = rule.Process(DateTime.UtcNow);

            SuccessAssert(result);

            Assert.IsTrue(Exists("Today.txt"));
            Assert.IsTrue(Exists("Today.csv"));
            Assert.IsTrue(Exists("Tomorrow.txt"));
            Assert.IsTrue(Exists("Tomorrow.csv"));
            Assert.IsTrue(Exists("Future7Days.txt"));
            Assert.IsTrue(Exists("Future7Days.csv"));

            Assert.IsFalse(Exists("Yesterday.txt"));
            Assert.IsFalse(Exists("Yesterday.csv"));
            Assert.IsFalse(Exists("Past7Days.txt"));
            Assert.IsFalse(Exists("Past7Days.csv"));
            Assert.IsTrue(Exists("Hidden.txt"));
            Assert.IsTrue(Exists("Hidden.csv"));
        }
        
        [TestMethod]
        public void DeleteAllTxtFilesBefore7DaysAgo()
        {
            var rule = new PurgeRule
            {
                DirectoryPath = TempPath,
                DaysToPurgeAfter = 7,
                MatchPattern = "^.*\\.txt?$"
            };

            var result = rule.Process(DateTime.UtcNow);

            SuccessAssert(result);

            Assert.IsTrue(Exists("Today.txt"));
            Assert.IsTrue(Exists("Today.csv"));
            Assert.IsTrue(Exists("Tomorrow.txt"));
            Assert.IsTrue(Exists("Tomorrow.csv"));
            Assert.IsTrue(Exists("Future7Days.txt"));
            Assert.IsTrue(Exists("Future7Days.csv"));

            Assert.IsTrue(Exists("Yesterday.txt"));
            Assert.IsTrue(Exists("Yesterday.csv"));
            Assert.IsFalse(Exists("Past7Days.txt"));
            Assert.IsTrue(Exists("Past7Days.csv"));
            Assert.IsTrue(Exists("Hidden.txt"));
            Assert.IsTrue(Exists("Hidden.csv"));
        }

        [TestMethod]
        public void DeleteHiddenFiles()
        {
            var rule = new PurgeRule
            {
                DirectoryPath = TempPath,
                DaysToPurgeAfter = 1,
                IncludeHiddenFiles = true
            };

            var result = rule.Process(DateTime.UtcNow);

            SuccessAssert(result);

            Assert.IsTrue(Exists("Today.txt"));
            Assert.IsTrue(Exists("Today.csv"));
            Assert.IsTrue(Exists("Tomorrow.txt"));
            Assert.IsTrue(Exists("Tomorrow.csv"));
            Assert.IsTrue(Exists("Future7Days.txt"));
            Assert.IsTrue(Exists("Future7Days.csv"));

            Assert.IsFalse(Exists("Yesterday.txt"));
            Assert.IsFalse(Exists("Yesterday.csv"));
            Assert.IsFalse(Exists("Past7Days.txt"));
            Assert.IsFalse(Exists("Past7Days.csv"));
            Assert.IsFalse(Exists("Hidden.txt"));
            Assert.IsFalse(Exists("Hidden.csv"));
        }

        [TestMethod]
        public void DeleteByLastModifiedTime()
        {
            var rule = new PurgeRule
            {
                DirectoryPath = TempPath,
                DaysToPurgeAfter = 1,
                TimeComparison = TimeComparison.LastWrite
            };

            var fileInfo = new FileInfo(Path.Combine(TempPath, "Past7Days.txt"))
            {
                LastWriteTimeUtc = DateTime.UtcNow
            };

            Assert.AreEqual(StartingFileCount, FileCount);

            var result = rule.Process(DateTime.UtcNow);

            SuccessAssert(result);

            Assert.IsTrue(Exists("Today.txt"));
            Assert.IsTrue(Exists("Today.csv"));
            Assert.IsTrue(Exists("Tomorrow.txt"));
            Assert.IsTrue(Exists("Tomorrow.csv"));
            Assert.IsTrue(Exists("Future7Days.txt"));
            Assert.IsTrue(Exists("Future7Days.csv"));

            Assert.IsFalse(Exists("Yesterday.txt"));
            Assert.IsFalse(Exists("Yesterday.csv"));
            Assert.IsTrue(Exists("Past7Days.txt"));
            Assert.IsFalse(Exists("Past7Days.csv"));
            Assert.IsTrue(Exists("Hidden.txt"));
            Assert.IsTrue(Exists("Hidden.csv"));
        }

        [TestMethod]
        public void DeleteAndKeepMinimumFiles()
        {
            var rule = new PurgeRule
            {
                DirectoryPath = TempPath,
                DaysToPurgeAfter = 1,
                MinimumFilesToKeep = 8
            };
            
            Assert.AreEqual(StartingFileCount, FileCount);

            var result = rule.Process(DateTime.UtcNow);

            SuccessAssert(result);

            Assert.IsTrue(Exists("Today.txt"));
            Assert.IsTrue(Exists("Today.csv"));
            Assert.IsTrue(Exists("Tomorrow.txt"));
            Assert.IsTrue(Exists("Tomorrow.csv"));
            Assert.IsTrue(Exists("Future7Days.txt"));
            Assert.IsTrue(Exists("Future7Days.csv"));

            // Yesterday was written to the file system before the rest of the
            // potential files to delete, so it will be kept
            Assert.IsTrue(Exists("Yesterday.txt"));
            Assert.IsTrue(Exists("Yesterday.csv"));

            Assert.IsFalse(Exists("Past7Days.txt"));
            Assert.IsFalse(Exists("Past7Days.csv"));
            Assert.IsTrue(Exists("Hidden.txt"));
            Assert.IsTrue(Exists("Hidden.csv"));
        }

        private void SuccessAssert(PurgeResult result)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(ExitCode.Success, result.RuleExitCode);
            Assert.AreEqual(true, result.WasDirectoryPurged);
            Assert.IsTrue(result.FilesDeletedCount > 0);

            Assert.IsTrue(FileCount < StartingFileCount);
            Assert.AreEqual(result.FilesDeletedCount, StartingFileCount - FileCount);
        }
    }
}
