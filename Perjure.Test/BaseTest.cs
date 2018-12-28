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
            CreateFile("Today.csv");

            CreateFile("Tomorrow.txt", Now(1));
            CreateFile("Tomorrow.csv", Now(1));
            CreateFile("Future7Days.txt", Now(7));
            CreateFile("Future7Days.csv", Now(7));

            CreateFile("Yesterday.txt", Now(-1));
            CreateFile("Yesterday.csv", Now(-1));
            CreateFile("Past7Days.txt", Now(-7));
            CreateFile("Past7Days.csv", Now(-7));
            CreateFile("Hidden.txt", Now(-7), true);
            CreateFile("Hidden.csv", Now(-7), true);

            StartingFileCount = FileCount;
        }

        [TestCleanup]
        public void TestCleanup() => Directory.Delete(TempPath, true);

        protected bool Exists(string fileName) => File.Exists(Path.Combine(TempPath, fileName));

        private DateTime Now(int daysFromNow) => DateTime.UtcNow.AddDays(daysFromNow);

        private void CreateFile(string fileName, DateTime? allAttributeTime = null, bool isHidden = false, long? size = null)
        {
            var path = Path.Combine(TempPath, fileName);

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                if (size.HasValue)
                {
                    fs.SetLength(size.Value);
                }

            }

            var now = DateTime.UtcNow;
            var fileInfo = new FileInfo(path)
            {
                CreationTimeUtc = allAttributeTime ?? now,
                LastWriteTimeUtc = allAttributeTime ?? now,
                LastAccessTimeUtc = allAttributeTime ?? now,
            };

            if (isHidden)
            {
                fileInfo.Attributes |= FileAttributes.Hidden;
            }
            else
            {
                fileInfo.Attributes &= ~FileAttributes.Hidden;
            }
        }
    }
}
