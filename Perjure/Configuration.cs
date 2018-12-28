using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace Perjure
{
    internal static class Configuration
    {
        private const string ConfigurationFileName = "Configuration.json";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal static List<PurgeRule> Load(string[] args)
        {
            var filePath = GetFilePath(args);
            Log.Debug("Using settings file '{0}'", filePath);

            return JsonConvert.DeserializeObject<List<PurgeRule>>(File.ReadAllText(filePath));
        }

        private static string GetFilePath(string[] args)
        {
            return args.Length == 1
                ? args[0]
                : Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), ConfigurationFileName);
        }
    }
}
