namespace LanguageServer
{
    public class CompileAndExecuteRequest
    {
        public CompileAndExecuteRequest(string source)
        {
            source = source ?? "";

            if (!source.IsFragment())
            {
                Sources = new[] { source };
            }
            else
            {
                Sources = new[]
                {
                    main,
                    $"{usingStatements} public static class Fragment {{ public static void Invoke() {{ {source} }} }}"
                };
            }
        }

        public string Language => "csharp";

        public string[] Sources { get; }

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