using System;
using System.Collections.Generic;
using System.Text;

namespace WorkspaceServer.Tests.CodeSamples
{
    internal static class SourceCodeProvider
    {
        public static string ConsoleProgramCollidingRegions =>
            @"using System;

namespace ConsoleProgramSingleRegion
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region alpha
            var a = 10;
            #endregion

            #region alpha
            var b = 10;
            #endregion
        }
    }
}";

        public static string ConsoleProgramSingleRegion =>
            @"using System;

namespace ConsoleProgramSingleRegion
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region alpha
            var a = 10;
            #endregion
        }
    }
}";


        public static string ConsoleProgramSingleRegionExtraUsing =>
            @"using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleProgramSingleRegion
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region alpha
            var a = 10;
            #endregion
        }
    }
}";
    }
}

