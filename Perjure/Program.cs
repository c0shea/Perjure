using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NLog;

namespace Perjure
{
    public static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string ConfigurationFileName = "Configuration.json";

        public static int Main(string[] args)
        {
            Log.Info("Started");

            var settingsFilePath = GetConfigurationFilePath(args);
            Log.Debug("Using settings file '{0}'", settingsFilePath);

            var rules = ReadRulesFromSettingsFile(settingsFilePath);
            if (rules == null)
            {
                return (int)ExitCode.InvalidConfiguration;
            }

            Log.Info("Processing rules...");
            var stopwatch = Stopwatch.StartNew();

            var purgeResults = ProcessRules(rules);

            stopwatch.Stop();

            Log.Info("{0} total seconds elapsed", stopwatch.Elapsed.TotalSeconds);
            Log.Info("{0} directories purged", purgeResults.Count(r => r.WasDirectoryPurged));
            Log.Info("{0} files deleted", purgeResults.Sum(r => r.FilesDeletedCount));
            
            var programExitCode = ExitCode.Success;
            foreach (var exitCode in purgeResults.Select(d => d.RuleExitCode).Distinct())
            {
                programExitCode |= exitCode;
            }

            if (programExitCode == ExitCode.Success)
            {
                Log.Info("Finished successfully");
            }
            else
            {
                Log.Error("One or more purge rules were not processed successfully. Finished with errors.");
            }

            return (int)programExitCode;
        }

        private static string GetConfigurationFilePath(string[] args)
        {
            return args.Length == 1
                    ? args[0]
                    : Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), ConfigurationFileName);
        }

        private static List<PurgeRule> ReadRulesFromSettingsFile(string settingsFilePath)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<PurgeRule>>(File.ReadAllText(settingsFilePath));
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to read configuration file or configuration is invalid.");
            }

            return null;
        }

        private static List<PurgeResult> ProcessRules(IEnumerable<PurgeRule> rules)
        {
            // In order to ensure consistency, all files are compared to the same DateTime
            var compareToDate = DateTime.UtcNow;

            return rules.Select(rule => rule.Process(compareToDate)).ToList();
        }
    }
}