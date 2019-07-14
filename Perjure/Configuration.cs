using System.Collections.Generic;

namespace Perjure
{
    public class Configuration
    {
        public bool IncludeRecycleBin { get; set; }
        public List<PurgeRule> PurgeRules { get; set; }
    }
}
