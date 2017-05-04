namespace WorkspaceServer
{
    public class BuildAndRunRequest
    {
        public BuildAndRunRequest(string source)
        {
            RawSource = source ?? "";

            if (!source.IsFragment())
            {
                Sources = new[] { RawSource };
            }
            else
            {
                Sources = new[]
                {
                    main,
                    $"{usingStatements} public static class Fragment {{ public static void Invoke() {{ {RawSource} }} }}"
                };
            }

        }

        public string Language => "csharp";

        public string[] Sources { get; }

        public string RawSource { get; }

        private static readonly string usingStatements = @"
using System; using System.Collections.Generic; using System.Linq;";

        private static readonly string main = usingStatements + @"
public class Program 
{
    public static void Main() {  
        Fragment.Invoke();
    }  
} ";
    }
}
