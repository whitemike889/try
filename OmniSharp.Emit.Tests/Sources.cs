using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OmniSharp.Emit.Tests
{

    internal class Sources
    {
        internal static SemanticModel GetSemanticModel(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location);
            MetadataReference console = MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location);
            MetadataReference runtime = MetadataReference.CreateFromFile(typeof(FileAttributes).GetTypeInfo().Assembly.Location);
            MetadataReference linq = MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location);
            MetadataReference io = MetadataReference.CreateFromFile(typeof(File).GetTypeInfo().Assembly.Location);

            var compilation = CSharpCompilation.Create("Program", new[] { syntaxTree }, new[] { mscorlib, console, runtime, linq, io });

            return compilation.GetSemanticModel(syntaxTree);
        }


        internal static readonly string empty = @"
using System;
namespace RoslynRecorder
{
    class Program
    {
        static void Main()
        {
        }
    }
}";

        internal static readonly string simple = @"
using System;
namespace RoslynRecorder
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(""Entry Point"");
        }
    }
}";

        internal static readonly string withLocalsAndParams = @"
using System;
namespace RoslynRecorder
{
    class Program
    {
        static void Main(string[] args)
        {
            int a = 0;
            Console.WriteLine(""Entry Point"");
        }
    }
}";

        internal static readonly string withNonAssignedLocals = @"
using System;
namespace RoslynRecorder
{
    class Program
    {
        static void Main()
        {
            string s;
            int a = 0;
            Console.WriteLine(""Entry Point"");
            s = ""now has a value"";
            a = 3;
        }
    }
}";

        internal static readonly string withStaticAndNonStaticField = @"
using System;
namespace RoslynRecorder
{
    class Program
    {
        static int a = 0;
        int b = 0;

        static void Main()
        {
            Console.WriteLine(""Entry Point"");
        }

        void InstanceMain()
        {
            Console.WriteLine(""Instance"");
        }
    }
}";
        internal static readonly string withMultipleMethodsAndComplexLayout = @"
using System;
namespace RoslynRecorder
{
    class Program
    {
        static int a = 0;
        int b = 0;

        static void Main(string[] args)
        {
            Console.WriteLine(""Entry Point"");
            var p = new Program();
            p.InstanceMain();
            
            int j = 0;
            int k;
            for(int i = 0; i < 10; i++)
            {
                j++;
            }
        }

        void InstanceMain()
        {
            Console.WriteLine(""Instance"");
        }
    }
}";



    }
}
