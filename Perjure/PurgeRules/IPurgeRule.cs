using System;

namespace Perjure.PurgeRules
{
    public interface IPurgeRule
    {
        void Process(DateTime compareToDate);
    }
}
