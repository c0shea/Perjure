using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Perjure {
    public class Program {
        public static int Main(string[] args) {
            var startTime = DateTime.UtcNow;
            Console.WriteLine($"Program started at:       {startTime.ToString("MM/dd/yyyy hh:mm:ss tt")} UTC");

            var settingsFilePath = GetSettingsFilePath(args);
            Console.WriteLine($"Settings file:            {settingsFilePath}");

            var rules = ReadRulesFromSettingsFile(settingsFilePath);

            if (rules == null) {
                return (int)ExitCode.InvalidConfiguration;
            }

            Console.WriteLine("Processing rules...\n");
            var purgeResults = ProcessRules(rules).ToList();

            Console.WriteLine($"\nTotal directories purged: {purgeResults.Count(d => d.WasDirectoryPurged)}");
            Console.WriteLine($"Total files deleted:      {purgeResults.Sum(d => d.FilesDeletedCount)}");
            
            var endTime = DateTime.UtcNow;
            Console.WriteLine($"Program ended at:         {endTime.ToString("MM/dd/yyyy hh:mm:ss tt")} UTC");
            Console.WriteLine($"Total seconds elapsed:    {(endTime - startTime).TotalSeconds.ToString("N")}");

            var programExitCode = ExitCode.Success;
            foreach (var exitCode in purgeResults.Select(d => d.RuleExitCode).Distinct())
            {
                programExitCode |= exitCode;
            }

            return (int)programExitCode;
        }

        private static string GetSettingsFilePath(string[] args) {
            return args.Length == 1
                    ? args[0]
                    : Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Settings.json";
        }

        private static List<PurgeRule> ReadRulesFromSettingsFile(string settingsFilePath) {
            try {
                var rules = JsonConvert.DeserializeObject<List<PurgeRule>>(File.ReadAllText(settingsFilePath));
                return rules;
            }
            catch (Exception ex) {
                Console.WriteLine($"ERROR: Invalid configuration. {ex.Message}");
            }

            return null;
        }

        private static IEnumerable<Statistic> ProcessRules(IEnumerable<PurgeRule> rules) {
            return rules.Select(rule => rule.Process());
        }
    }

    [Flags]
    public enum ExitCode {
        Success = 0,
        InvalidConfiguration = 1,
        DirectoryNotFound = 2,
        FileNotDeleted = 4
    }
}