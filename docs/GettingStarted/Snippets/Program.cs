using System;
using System.Linq;
using System.CommandLine;
namespace Snippets
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var rootCommand = new RootCommand()
            {
                new Option("--region", argument: new Argument<string>()),
                new Option("--session", argument: new Argument<string>()),
                new Option("--package", argument: new Argument<string>()),
                new Option("--project", argument: new Argument<string>())
            };
            rootCommand.Argument = new Argument<string>
            {
                Arity = ArgumentArity.ZeroOrMore
            };
            var parser = new Parser(rootCommand);
            var region = parser.Parse(args).ValueForOption<string>("--region");
            
            switch (region)
            {
                    case "run":
                    Run();
                    break;
                    case "run1":
                    Run1();
                    break;
                    case "run2":
                    Run2();
                    break;
                    case "run3":
                    Run2();
                    break;
                 }
            }
            public static void Run()
            {
                #region run
                Console.WriteLine("Hello World");
                #endregion
            }
            public static void Run1()
            {
                #region run1
                Console.WriteLine(DateTime.Now);
                #endregion
            }
            public static void Run2()
            {
                #region run2
                Console.WriteLine(Guid.NewGuid());
                #endregion
            }
            public static void Run3()
            {
                #region run3
                // something something
                #endregion
            }
    }
}