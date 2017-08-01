using System;
using System.Collections.Generic;

namespace MLS.Agent.Tests
{
    public class LogEntryList : List<(
        int LogLevel,
        DateTimeOffset Timestamp,
        Func<(string Message, (string Name, object Value)[] Properties)> Evaluate,
        Exception Exception,
        string OperationName,
        string Category,
        (string Id,
        bool IsStart,
        bool IsEnd,
        bool? IsSuccessful,
        TimeSpan? Duration) Operation)>
    {
    }
}
