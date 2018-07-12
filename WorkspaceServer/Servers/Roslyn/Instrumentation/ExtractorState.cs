using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    internal class ExtractorState
    {
        public readonly bool IsInstrumentation;
        public readonly string ProgramDescriptor;
        public readonly ImmutableList<string> Instrumentation;
        public readonly ImmutableList<string> StdOut;

        public ExtractorState(
            bool isInstrumentation = false,
            string programDescriptor = "",
            ImmutableList<string> instrumentation = null,
            ImmutableList<string> stdOut = null)
        {
            IsInstrumentation = isInstrumentation;
            ProgramDescriptor = programDescriptor;
            Instrumentation = instrumentation ?? ImmutableList.Create<string>();
            StdOut = stdOut ?? ImmutableList.Create<string>();
        }

        public ExtractorState With(bool? isInstrumentation = null,
            string programDescriptor = "",
            ImmutableList<string> instrumentation = null,
            ImmutableList<string> stdOut = null)
        {
            return new ExtractorState(
                isInstrumentation ?? this.IsInstrumentation,
                programDescriptor == "" ? this.ProgramDescriptor : programDescriptor,
                instrumentation ?? this.Instrumentation,
                stdOut ?? this.StdOut
            );
        }
    }
}
