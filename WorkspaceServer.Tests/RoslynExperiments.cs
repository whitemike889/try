using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Xunit;
using WorkspaceServer.Servers.Local;

namespace WorkspaceServer.Tests
{
    public class RoslynExperiments
    {
        [Fact(Skip = "not now")]
        public void compile_without_project()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
using System;

public static class C
{
    public static void Main()
    {
        Console.WriteLine(""Hello World!"");
    }   
}");

            var compilation = CSharpCompilation.Create(
                "MyCompilation",
                syntaxTrees: new[] { tree },
                references: references.Select(r => MetadataReference.CreateFromFile(r)));

            if (File.Exists("output.dll"))
            {
                File.Delete("output.dll");
            }

            //Emitting to file is available through an extension method in the Microsoft.CodeAnalysis namespace
            var emitResult = compilation.Emit("output.dll");

            //If our compilation failed, we can discover exactly why.
            if (!emitResult.Success)
            {
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    Console.WriteLine(diagnostic.ToString());
                }
            }

            var workingDir = Directory.GetCurrentDirectory();
            Console.WriteLine(new { workingDir });
            var dotnet = new Dotnet(new DirectoryInfo(workingDir));

            var result = dotnet.ExecuteDotnet("exec output.dll");

            Console.WriteLine(result);

            // TODO-JOSEQU (testname) write test
            Assert.True(false, "Test testname is not written yet.");
        }

        [Fact(Skip = "not now")]
        public async Task script_evaluate()
        {
            var result = await CSharpScript.EvaluateAsync("5 + 5");
            Console.WriteLine(result); // 10

            result = await CSharpScript.EvaluateAsync(@"""sample""");
            Console.WriteLine(result); // sample

            result = await CSharpScript.EvaluateAsync(@"""sample"" + "" string""");
            Console.WriteLine(result); // sample string

            result = await CSharpScript.EvaluateAsync("int x = 5; int y = 5; x"); //Note the last x is not contained in a proper statement

            Console.WriteLine(result); // 5
        }

        [Fact(Skip = "not now")]
        public async Task script_expressions()
        {
            var scriptOptions = ScriptOptions.Default;

            //Add reference to mscorlib
            var mscorlib = typeof(Object).GetTypeInfo().Assembly;
            var systemCore = typeof(Enumerable).GetTypeInfo().Assembly;
            scriptOptions = scriptOptions.AddReferences(mscorlib, systemCore);

            //Add namespaces
            scriptOptions = scriptOptions.AddImports("System");
            scriptOptions = scriptOptions.AddImports("System.Linq");
            scriptOptions = scriptOptions.AddImports("System.Collections.Generic");

            var state = await CSharpScript.RunAsync(@"
var x = new List<int>(){1,2,3,4,5};", scriptOptions);
            state = await state.ContinueWithAsync("var y = x.Take(3).ToList();");
            var y = state.Variables.Single(v => v.Name == "y");
            var yList = (List<int>) y.Value;

            foreach (var val in yList)
            {
                Console.WriteLine(val + " "); // Prints 1 2 3
            }
        }

        [Fact(Skip = "not now")]
        public void script_create()
        {
            var x = CSharpScript.Create(
                @"using System;

public static class C
{
    public static void Main()
    {
        Console.WriteLine(""""Hello World!"""");
    }   
}""");

            var compilation = x.GetCompilation();

            // TODO-JOSEQU (script_create) write test
            Assert.True(false, "Test script_create is not written yet.");
        }

        [Fact(Skip = "not now")]
        public async Task console()
        {
            var x = await CSharpScript.EvaluateAsync(@"System.Console.WriteLine(""Hello world!"");");

            Console.WriteLine($"RESULT: {x}");

            // TODO-JOSEQU (console) write test
            Assert.True(false, "Test console is not written yet.");
        }

        private readonly string[] references = new[]
        {
            @"C:\Users\josequ\.nuget\packages\microsoft.csharp\4.3.0\ref\netstandard1.0\Microsoft.CSharp.dll ",
            @"C:\Users\josequ\.nuget\packages\microsoft.visualbasic\10.1.0\ref\netstandard1.1\Microsoft.VisualBasic.dll ",
            @"C:\Users\josequ\.nuget\packages\microsoft.win32.primitives\4.3.0\ref\netstandard1.3\Microsoft.Win32.Primitives.dll ",
            @"C:\Users\josequ\.nuget\packages\system.appcontext\4.3.0\ref\netstandard1.6\System.AppContext.dll ",
            @"C:\Users\josequ\.nuget\packages\system.buffers\4.3.0\lib\netstandard1.1\System.Buffers.dll ",
            @"C:\Users\josequ\.nuget\packages\system.collections.concurrent\4.3.0\ref\netstandard1.3\System.Collections.Concurrent.dll ",
            @"C:\Users\josequ\.nuget\packages\system.collections\4.3.0\ref\netstandard1.3\System.Collections.dll ",
            @"C:\Users\josequ\.nuget\packages\system.collections.immutable\1.3.0\lib\netstandard1.0\System.Collections.Immutable.dll ",
            @"C:\Users\josequ\.nuget\packages\system.componentmodel.annotations\4.3.0\ref\netstandard1.4\System.ComponentModel.Annotations.dll ",
            @"C:\Users\josequ\.nuget\packages\system.componentmodel\4.3.0\ref\netstandard1.0\System.ComponentModel.dll ",
            @"C:\Users\josequ\.nuget\packages\system.console\4.3.0\ref\netstandard1.3\System.Console.dll ",
            @"C:\Users\josequ\.nuget\packages\system.diagnostics.debug\4.3.0\ref\netstandard1.3\System.Diagnostics.Debug.dll ",
            @"C:\Users\josequ\.nuget\packages\system.diagnostics.diagnosticsource\4.3.0\lib\netstandard1.3\System.Diagnostics.DiagnosticSource.dll ",
            @"C:\Users\josequ\.nuget\packages\system.diagnostics.process\4.3.0\ref\netstandard1.4\System.Diagnostics.Process.dll ",
            @"C:\Users\josequ\.nuget\packages\system.diagnostics.tools\4.3.0\ref\netstandard1.0\System.Diagnostics.Tools.dll ",
            @"C:\Users\josequ\.nuget\packages\system.diagnostics.tracing\4.3.0\ref\netstandard1.5\System.Diagnostics.Tracing.dll ",
            @"C:\Users\josequ\.nuget\packages\system.dynamic.runtime\4.3.0\ref\netstandard1.3\System.Dynamic.Runtime.dll ",
            @"C:\Users\josequ\.nuget\packages\system.globalization.calendars\4.3.0\ref\netstandard1.3\System.Globalization.Calendars.dll ",
            @"C:\Users\josequ\.nuget\packages\system.globalization\4.3.0\ref\netstandard1.3\System.Globalization.dll ",
            @"C:\Users\josequ\.nuget\packages\system.globalization.extensions\4.3.0\ref\netstandard1.3\System.Globalization.Extensions.dll ",
            @"C:\Users\josequ\.nuget\packages\system.io.compression\4.3.0\ref\netstandard1.3\System.IO.Compression.dll ",
            @"C:\Users\josequ\.nuget\packages\system.io.compression.zipfile\4.3.0\ref\netstandard1.3\System.IO.Compression.ZipFile.dll ",
            @"C:\Users\josequ\.nuget\packages\system.io\4.3.0\ref\netstandard1.5\System.IO.dll ",
            @"C:\Users\josequ\.nuget\packages\system.io.filesystem\4.3.0\ref\netstandard1.3\System.IO.FileSystem.dll ",
            @"C:\Users\josequ\.nuget\packages\system.io.filesystem.primitives\4.3.0\ref\netstandard1.3\System.IO.FileSystem.Primitives.dll ",
            @"C:\Users\josequ\.nuget\packages\system.io.filesystem.watcher\4.3.0\ref\netstandard1.3\System.IO.FileSystem.Watcher.dll ",
            @"C:\Users\josequ\.nuget\packages\system.io.memorymappedfiles\4.3.0\ref\netstandard1.3\System.IO.MemoryMappedFiles.dll ",
            @"C:\Users\josequ\.nuget\packages\system.io.unmanagedmemorystream\4.3.0\ref\netstandard1.3\System.IO.UnmanagedMemoryStream.dll ",
            @"C:\Users\josequ\.nuget\packages\system.linq\4.3.0\ref\netstandard1.6\System.Linq.dll ",
            @"C:\Users\josequ\.nuget\packages\system.linq.expressions\4.3.0\ref\netstandard1.6\System.Linq.Expressions.dll ",
            @"C:\Users\josequ\.nuget\packages\system.linq.parallel\4.3.0\ref\netstandard1.1\System.Linq.Parallel.dll ",
            @"C:\Users\josequ\.nuget\packages\system.linq.queryable\4.3.0\ref\netstandard1.0\System.Linq.Queryable.dll ",
            @"C:\Users\josequ\.nuget\packages\system.net.http\4.3.0\ref\netstandard1.3\System.Net.Http.dll ",
            @"C:\Users\josequ\.nuget\packages\system.net.nameresolution\4.3.0\ref\netstandard1.3\System.Net.NameResolution.dll ",
            @"C:\Users\josequ\.nuget\packages\system.net.primitives\4.3.0\ref\netstandard1.3\System.Net.Primitives.dll ",
            @"C:\Users\josequ\.nuget\packages\system.net.requests\4.3.0\ref\netstandard1.3\System.Net.Requests.dll ",
            @"C:\Users\josequ\.nuget\packages\system.net.security\4.3.0\ref\netstandard1.3\System.Net.Security.dll ",
            @"C:\Users\josequ\.nuget\packages\system.net.sockets\4.3.0\ref\netstandard1.3\System.Net.Sockets.dll ",
            @"C:\Users\josequ\.nuget\packages\system.net.webheadercollection\4.3.0\ref\netstandard1.3\System.Net.WebHeaderCollection.dll ",
            @"C:\Users\josequ\.nuget\packages\system.numerics.vectors\4.3.0\ref\netstandard1.0\System.Numerics.Vectors.dll ",
            @"C:\Users\josequ\.nuget\packages\system.objectmodel\4.3.0\ref\netstandard1.3\System.ObjectModel.dll ",
            @"C:\Users\josequ\.nuget\packages\system.reflection.dispatchproxy\4.3.0\ref\netstandard1.3\System.Reflection.DispatchProxy.dll ",
            @"C:\Users\josequ\.nuget\packages\system.reflection\4.3.0\ref\netstandard1.5\System.Reflection.dll ",
            @"C:\Users\josequ\.nuget\packages\system.reflection.extensions\4.3.0\ref\netstandard1.0\System.Reflection.Extensions.dll ",
            @"C:\Users\josequ\.nuget\packages\system.reflection.metadata\1.4.1\lib\netstandard1.1\System.Reflection.Metadata.dll ",
            @"C:\Users\josequ\.nuget\packages\system.reflection.primitives\4.3.0\ref\netstandard1.0\System.Reflection.Primitives.dll ",
            @"C:\Users\josequ\.nuget\packages\system.reflection.typeextensions\4.3.0\ref\netstandard1.5\System.Reflection.TypeExtensions.dll ",
            @"C:\Users\josequ\.nuget\packages\system.resources.reader\4.3.0\lib\netstandard1.0\System.Resources.Reader.dll ",
            @"C:\Users\josequ\.nuget\packages\system.resources.resourcemanager\4.3.0\ref\netstandard1.0\System.Resources.ResourceManager.dll ",
            @"C:\Users\josequ\.nuget\packages\system.runtime\4.3.0\ref\netstandard1.5\System.Runtime.dll ",
            @"C:\Users\josequ\.nuget\packages\system.runtime.extensions\4.3.0\ref\netstandard1.5\System.Runtime.Extensions.dll ",
            @"C:\Users\josequ\.nuget\packages\system.runtime.handles\4.3.0\ref\netstandard1.3\System.Runtime.Handles.dll ",
            @"C:\Users\josequ\.nuget\packages\system.runtime.interopservices\4.3.0\ref\netcoreapp1.1\System.Runtime.InteropServices.dll ",
            @"C:\Users\josequ\.nuget\packages\system.runtime.interopservices.runtimeinformation\4.3.0\ref\netstandard1.1\System.Runtime.InteropServices.RuntimeInformation.dll ",
            @"C:\Users\josequ\.nuget\packages\system.runtime.numerics\4.3.0\ref\netstandard1.1\System.Runtime.Numerics.dll ",
            @"C:\Users\josequ\.nuget\packages\system.security.cryptography.algorithms\4.3.0\ref\netstandard1.6\System.Security.Cryptography.Algorithms.dll ",
            @"C:\Users\josequ\.nuget\packages\system.security.cryptography.encoding\4.3.0\ref\netstandard1.3\System.Security.Cryptography.Encoding.dll ",
            @"C:\Users\josequ\.nuget\packages\system.security.cryptography.primitives\4.3.0\ref\netstandard1.3\System.Security.Cryptography.Primitives.dll ",
            @"C:\Users\josequ\.nuget\packages\system.security.cryptography.x509certificates\4.3.0\ref\netstandard1.4\System.Security.Cryptography.X509Certificates.dll ",
            @"C:\Users\josequ\.nuget\packages\system.security.principal\4.3.0\ref\netstandard1.0\System.Security.Principal.dll ",
            @"C:\Users\josequ\.nuget\packages\system.text.encoding\4.3.0\ref\netstandard1.3\System.Text.Encoding.dll ",
            @"C:\Users\josequ\.nuget\packages\system.text.encoding.extensions\4.3.0\ref\netstandard1.3\System.Text.Encoding.Extensions.dll ",
            @"C:\Users\josequ\.nuget\packages\system.text.regularexpressions\4.3.0\ref\netcoreapp1.1\System.Text.RegularExpressions.dll ",
            @"C:\Users\josequ\.nuget\packages\system.threading\4.3.0\ref\netstandard1.3\System.Threading.dll ",
            @"C:\Users\josequ\.nuget\packages\system.threading.tasks.dataflow\4.7.0\lib\netstandard1.1\System.Threading.Tasks.Dataflow.dll ",
            @"C:\Users\josequ\.nuget\packages\system.threading.tasks\4.3.0\ref\netstandard1.3\System.Threading.Tasks.dll ",
            @"C:\Users\josequ\.nuget\packages\system.threading.tasks.extensions\4.3.0\lib\netstandard1.0\System.Threading.Tasks.Extensions.dll ",
            @"C:\Users\josequ\.nuget\packages\system.threading.tasks.parallel\4.3.0\ref\netstandard1.1\System.Threading.Tasks.Parallel.dll ",
            @"C:\Users\josequ\.nuget\packages\system.threading.thread\4.3.0\ref\netstandard1.3\System.Threading.Thread.dll ",
            @"C:\Users\josequ\.nuget\packages\system.threading.threadpool\4.3.0\ref\netstandard1.3\System.Threading.ThreadPool.dll ",
            @"C:\Users\josequ\.nuget\packages\system.threading.timer\4.3.0\ref\netstandard1.2\System.Threading.Timer.dll ",
            @"C:\Users\josequ\.nuget\packages\system.xml.readerwriter\4.3.0\ref\netstandard1.3\System.Xml.ReaderWriter.dll ",
            @"C:\Users\josequ\.nuget\packages\system.xml.xdocument\4.3.0\ref\netstandard1.3\System.Xml.XDocument.dll ",
        };
    }
}
