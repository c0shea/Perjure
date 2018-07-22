using System;

namespace Perjure
{
    [Flags]
    public enum ExitCode
    {
        Success = 0,
        InvalidConfiguration = 1,
        DirectoryNotFound = 2,
        FileNotDeleted = 4
    }
}
