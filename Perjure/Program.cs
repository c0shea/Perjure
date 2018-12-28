using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace Perjure
{
    public static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static int Main(string[] args)
        {
            Log.Info("Started");

            List<PurgeRule> rules;
            try
            {
                rules = Configuration.Load(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to read configuration file or configuration is invalid.");
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

        private static List<PurgeResult> ProcessRules(IEnumerable<PurgeRule> rules)
        {
            // In order to ensure consistency, all files are compared to the same DateTime
            var compareToDate = DateTime.UtcNow;

            return rules.Select(rule => rule.Process(compareToDate)).ToList();
        }
    }
}