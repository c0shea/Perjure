using System;
using System.Runtime.InteropServices;
using NLog;
using Perjure.Interop;

namespace Perjure.PurgeRules
{
    public class RecycleBinPurgeRule : IPurgeRule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Process(DateTime compareToDate)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Log.Warn("Emptying the recycle bin is only supported on Windows.");
            }

            try
            {
                RecycleBin.Empty();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to empty the recycle bin.");
            }
        }
    }
}
