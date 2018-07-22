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

            Assert.IsNotNull(result);
            Assert.AreEqual(ExitCode.Success, result.RuleExitCode);
            Assert.AreEqual(true, result.WasDirectoryPurged);
            Assert.IsTrue(result.FilesDeletedCount > 0);

            Assert.IsTrue(FileCount < StartingFileCount);
            Assert.AreEqual(result.FilesDeletedCount, StartingFileCount - FileCount);

            Assert.IsTrue(Exists("Today.txt"));
            Assert.IsTrue(Exists("Tomorrow.txt"));
            Assert.IsTrue(Exists("Future7Days.txt"));
            Assert.IsFalse(Exists("Yesterday.txt"));
            Assert.IsFalse(Exists("Past7Days.txt"));
        }
    }
}
