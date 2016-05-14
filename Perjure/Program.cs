using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Perjure {
    public class Program {
        public static void Main(string[] args) {
            var startTime = DateTime.UtcNow;
            Console.WriteLine($"Program started at {startTime.ToString("MM/dd/yyyy hh:mm:ss tt")} UTC");

            var settingsFilePath = GetSettingsFilePath(args);
            Console.WriteLine($"Settings file: {settingsFilePath}");

            var rules = ReadRulesFromSettingsFile(settingsFilePath);

            Console.WriteLine("Processing rules...\n");
            ProcessRules(rules);

            var endTime = DateTime.UtcNow;
            Console.WriteLine($"\nProgram ended at {endTime.ToString("MM/dd/yyyy hh:mm:ss tt")} UTC");
            Console.WriteLine($"Total seconds elapsed: {(endTime - startTime).TotalSeconds.ToString("N")}");

            Console.ReadLine();
        }

        private static string GetSettingsFilePath(string[] args) {
            return args.Length == 1
                    ? args[0]
                    : Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Settings.json";
        }

        private static List<PurgeRule> ReadRulesFromSettingsFile(string settingsFilePath) {
            return JsonConvert.DeserializeObject<List<PurgeRule>>(File.ReadAllText(settingsFilePath));
        }

        private static void ProcessRules(List<PurgeRule> rules) {
            foreach (var rule in rules) {
                rule.Process();
            }
        }
    }
}