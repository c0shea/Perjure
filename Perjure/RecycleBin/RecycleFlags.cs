using System;
// ReSharper disable InconsistentNaming

namespace Perjure.RecycleBin
{
    /// <remarks>
    /// https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shemptyrecyclebinw
    /// </remarks>
    [Flags]
    internal enum RecycleFlags : uint
    {
        None = 0x00000000,

        /// <summary>
        /// No dialog box confirming the deletion of the objects will be displayed.
        /// </summary>
        SHERB_NOCONFIRMATION = 0x00000001,

        /// <summary>
        /// No dialog box indicating the progress will be displayed.
        /// </summary>
        SHERB_NOPROGRESSUI = 0x00000002,

        /// <summary>
        /// No sound will be played when the operation is complete.
        /// </summary>
        SHERB_NOSOUND = 0x00000004
    }
}
