using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace Perjure
{
    public static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static Configuration _configuration;

        public static int Main(string[] args)
        {
            Log.Info("Started");

            try
            {
                _configuration = LoadConfiguration(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to read configuration file or configuration is invalid");
                return (int)ExitCode.InvalidConfiguration;
            }

            if (ProcessRules())
            {
                Log.Info("Finished successfully");
                return (int)ExitCode.Success;
            }

            Log.Error("One or more purge rules were not processed successfully. Finished with errors.");
            return (int)ExitCode.RuleError;
        }

        private static Configuration LoadConfiguration(string[] args)
        {
            var filePath = args != null && args.Length == 1
                ? args[0]
                : Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Configuration.json");

            Log.Debug("Using settings file {FilePath}", filePath);

            return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(filePath));
        }

        /// <summary>
        /// Returns true if all of the purge rules executed sucessfully, false otherwise.
        /// </summary>
        private static bool ProcessRules()
        {
            Log.Info("Processing rules");

            // In order to ensure consistency, all rules use the same DateTime to determine age
            var compareToDate = DateTime.UtcNow;
            var wasSuccessful = true;
            var stopwatch = Stopwatch.StartNew();


            foreach (var purgeRule in _configuration.GetAllPurgeRules())
            {
                try
                {
                    purgeRule.Process(compareToDate);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred processing the {RuleName}", purgeRule.GetType().Name);
                    wasSuccessful = false;
                }
            }

            stopwatch.Stop();
            Log.Info("Processed rules in {ElapsedTime}", stopwatch.Elapsed);

            return wasSuccessful;
        }
    }
}
