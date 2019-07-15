using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using NLog;
using Perjure.Interop;

namespace Perjure.PurgeRules
{
    public class RecycleBinPurgeRule : IPurgeRule
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public void Process(DateTime compareToDate)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Log.Warn("Emptying the recycle bin is only supported on Windows.");
                return;
            }

            Log.Info("Emptying the recycle bin");

            var emptyRecycleBinStatus = RecycleBin.SHEmptyRecycleBin(
                IntPtr.Zero, 
                null, 
                RecycleFlags.SHERB_NOCONFIRMATION | RecycleFlags.SHERB_NOPROGRESSUI | RecycleFlags.SHERB_NOSOUND);

            if (emptyRecycleBinStatus != HRESULT.S_OK)
            {
                throw new Win32Exception((int)emptyRecycleBinStatus);
            }

            Log.Info("Finished emptying the recycle bin");
        }
    }
}
