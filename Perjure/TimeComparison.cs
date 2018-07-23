namespace Perjure
{
    /// <summary>
    /// The types of ways that a purge rule is compared to the files in the directory.
    /// </summary>
    public enum TimeComparison
    {
        Creation,
        LastWrite,
        LastAccess
    }
}
