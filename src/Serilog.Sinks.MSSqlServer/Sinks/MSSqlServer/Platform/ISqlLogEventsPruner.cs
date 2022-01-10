using System;
using System.Data;
using System.Threading.Tasks;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal interface ISqlLogEventsPruner
    {
         Task PruneLogEventsToDateTime();
    }
}
