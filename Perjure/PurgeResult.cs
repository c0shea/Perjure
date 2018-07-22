namespace Perjure
{
    public class PurgeResult
    {
        public bool WasDirectoryPurged { get; set; }
        public long FilesDeletedCount { get; set; }
        public ExitCode RuleExitCode { get; set; }
    }
}