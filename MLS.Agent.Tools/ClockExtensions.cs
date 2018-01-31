using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Clockwise;
using Pocket;
using static Pocket.Logger;

namespace MLS.Agent.Tools
{
    public static class ClockExtensions
    {
        public static CancellationToken CreateCancellationToken(
            this IClock clock,
            TimeSpan cancelAfter,
            [CallerMemberName] string operationName = null)
        {
            var source = new CancellationTokenSource();
            
            clock.Schedule(c =>
            {
                Log.Trace($"Canceling {cancelAfter.TotalSeconds}s CancellationToken created by {operationName}", operationName);

                source.Cancel();

            }, cancelAfter);

            return source.Token;
        }
    }
}
