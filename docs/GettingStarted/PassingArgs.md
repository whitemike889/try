# Passing arguments to your sample project

Now that you have a few different regions and few different sessions, you might want to actually call different code depending on which button was clicked. Remember, `dotnet try` will replace some code and then invoke your program's `Main`. But you may not want to run all of the code in your program for every button. If you can switch code paths based on which button was clicked, you can get a lot more use out of that sample project.

You may have noticed that the signature of `Program.Main` in the [QuickStart](./QuickStart.md)'s backing project (`Snippets.csproj`) looks a little strange:

```cs
static void Main(
    string region = null,
    string session = null,
    string package = null,
    string project = null,
    string[] args = null)
{
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
            Run3();
            break;
    }
}
```

Instead of the familiar `Main(string[] args)` entry point, this program's entry point uses the new [experimental library](https://github.com/dotnet/command-line-api/wiki/DragonFruit-overview) `System.CommandLine.DragonFruit` to parse the arguments that were specified in your Markdown file's code fence. The `QuickStart.md` sample uses these arguments to route to different methods, but you can probably think of other ways to use these arguments. As you saw from the tutorial, you're not required to use any particular library in your backing project. But the command line arguments are available if you want to respond to them, and `DragonFruit` is a concise option for doing so.

_Congratulations! You've finished the `dotnet try` step-by-step tutorial walkthrough._

**[< Verify your project](./Verify.md)**
