using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace Perjure.Interop
{
    internal static class RecycleBin
    {
        /// <summary>
        /// Empties the Recycle Bin on the specified drive.
        /// </summary>
        /// <param name="hwnd">
        /// A handle to the parent window of any dialog boxes that might be displayed during the operation.
        /// This parameter can be NULL.
        /// </param>
        /// <param name="pszRootPath">
        /// The address of a null-terminated string of maximum length MAX_PATH that contains the path of the root drive on
        /// which the Recycle Bin is located. This parameter can contain the address of a string formatted with the drive,
        /// folder, and subfolder names, for example c:\windows\system. It can also contain an empty string or NULL.
        /// If this value is an empty string or NULL, all Recycle Bins on all drives will be emptied.
        /// </param>
        /// <param name="dwFlags">One or more of the following values.</param>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shemptyrecyclebinw
        /// </remarks>
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern HRESULT SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);
    }
}
