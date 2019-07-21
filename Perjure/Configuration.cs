using System.Collections.Generic;
using Perjure.PurgeRules;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Perjure
{
    public class Configuration
    {
        public bool IncludeRecycleBin { get; set; }
        public InternetExplorerPurgeOptions InternetExplorerPurgeOptions { get; set; }
        public List<FilePurgeRule> FilePurgeRules { get; set; }

        public IEnumerable<IPurgeRule> GetAllPurgeRules()
        {
            if (IncludeRecycleBin)
            {
                yield return new RecycleBinPurgeRule();
            }

            if (InternetExplorerPurgeOptions != InternetExplorerPurgeOptions.None)
            {
                yield return new InternetExplorerPurgeRule(InternetExplorerPurgeOptions);
            }

            if (FilePurgeRules != null)
            {
                foreach (var purgeRule in FilePurgeRules)
                {
                    yield return purgeRule;
                }
            }
        }
    }
}
