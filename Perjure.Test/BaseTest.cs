using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Perjure.Test
{
    public class BaseTest
    {
        protected string TempPath { get; private set; }
        protected int StartingFileCount { get; private set; }
        protected int FileCount => Directory.GetFiles(TempPath).Length;

        [TestInitialize]
        public void TestInitialize()
        {
            TempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(TempPath);

            CreateFile("Today.txt");
            CreateFile("Yesterday.txt", Now(-1));
            CreateFile("Tomorrow.txt", Now(1));
            CreateFile("Future7Days.txt", Now(7));
            CreateFile("Past7Days.txt", Now(-7));

            StartingFileCount = 5;
        }

        [TestCleanup]
        public void TestCleanup() => Directory.Delete(TempPath, true);

        protected bool Exists(string fileName) => File.Exists(Path.Combine(TempPath, fileName));

        private DateTime Now(int daysFromNow) => DateTime.UtcNow.AddDays(daysFromNow);

        private void CreateFile(string fileName, DateTime? allAttributeTime = null)
        {
            var path = Path.Combine(TempPath, fileName);

            using (File.Create(path))
            {
            }

            var now = DateTime.UtcNow;
            var fileInfo = new FileInfo(path)
            {
                CreationTimeUtc = allAttributeTime ?? now,
                LastWriteTimeUtc = allAttributeTime ?? now,
                LastAccessTimeUtc = allAttributeTime ?? now
            };
        }
    }
}
